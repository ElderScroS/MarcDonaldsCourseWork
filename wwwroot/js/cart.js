function updateTotalPrice() {
    const $orderButton = $(".order-button");

    $orderButton.css("opacity", "0.6").css("pointer-events", "none");

    setTimeout(() => {
        let subtotal = 0;
        const totalItems = $(".product-card").length;

        $(".product-card").each(function () {
            const quantity = parseInt($(this).find(".product-quantity").text(), 10);
            const priceText = $(this).find(".product-info p").text().replace('AZN', '').trim().replace(',', '.');
            const price = parseFloat(priceText);
            subtotal += quantity * price;
        });

        $(".product-subtotal").text(`${subtotal.toFixed(2).replace('.', ',')} AZN`);

        const serviceFee = 0.8;
        const packagingFee = 0.5;

        const deliveryText = $(".summary-line.delivery-cost span").text().replace('AZN', '').trim().replace(',', '.');
        const deliveryCost = isNaN(parseFloat(deliveryText)) ? 0 : parseFloat(deliveryText);

        // Считаем полную сумму без скидки
        let totalPrice = subtotal + serviceFee + packagingFee + deliveryCost;

        // Применяем скидку к полной сумме
        if (totalItems >= 4) {
            totalPrice *= 0.7;  // 30% скидка
            $(".discount").show();
        } else {
            $(".discount").hide();
        }

        // Обновляем отображение чаевых на все проценты (5,10,15) относительно суммы без скидки
        const tipPercentages = [5, 10, 15];
        tipPercentages.forEach(percent => {
            const tipAmount = totalPrice * (percent / 100);
            const formattedTip = `${tipAmount.toFixed(2).replace('.', ',')} AZN`;
            $(`.${percent}tipsAmount`).text(formattedTip);
        });

        // Чаевые выбранной кнопки
        let tipAmount = 0;
        const selectedTipBtn = $(".tip-btn.selected");
        if (selectedTipBtn.length > 0) {
            const percent = parseInt(selectedTipBtn.find("span[id='percent']").text().replace('%', ''), 10);
            if (!isNaN(percent) && percent > 0) {
                tipAmount = totalPrice * (percent / 100);
                const formattedTip = `${tipAmount.toFixed(2).replace('.', ',')} AZN`;

                $(".tips-amount").text(formattedTip);

                if ($(".tips-summary-line").length === 0) {
                    const tipHTML = `
                        <p class="summary-line tips-summary-line">
                            Tip for courier: <span>${formattedTip}</span>
                        </p>
                    `;
                    $(".summary-line.delivery-cost").after(tipHTML);
                } else {
                    $(".tips-summary-line span").text(formattedTip);
                }
            } else {
                $(".tips-summary-line").remove();
                $(".tips-amount").text(`0,00 AZN`);
            }
        }

        // Итоговая сумма с учётом чаевых
        const finalTotal = totalPrice + tipAmount;
        const formattedFinal = `${finalTotal.toFixed(2).replace('.', ',')} AZN`;

        $(".summary-total .final-price").text(formattedFinal);
        $(".order-button .order-price").text(formattedFinal);

        $orderButton.css("opacity", "1").css("pointer-events", "auto");
    }, 300);
}

$(document).ready(function () {
    $(".tip-btn").on("click", function () {
        $(".tip-btn").removeClass("selected");
        $(this).addClass("selected");

        const percentText = $(this).find("span[id='percent']").text().replace('%', '');
        const percent = parseInt(percentText, 10);
        const amountText = $(this).find("span[id='amount']").text().replace('AZN', '').trim().replace(',', '.');
        const amount = parseFloat(amountText);

        $(".tips-amount").text(`${amount.toFixed(2).replace('.', ',')} AZN`);

        $(".tips-summary-line").remove();

        if (percent > 0) {
            const tipSummaryHTML = `
                <p class="summary-line tips-summary-line">
                    Tip for courier: <span>${amount.toFixed(2).replace('.', ',')} AZN</span>
                </p>
            `;
            $(".summary-line").last().after(tipSummaryHTML);
        }

        const tips = {
            TipsPercentage: percent,
            TipsAmount: amount
        };

        $.ajax({
            url: '/User/SelectTips', 
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(tips),
            success: function () {
                updateTotalPrice();
            },
            error: function (xhr, status, error) {
            }
        });
    });

    function showFloatingQuantity($element, quantity) {
        const $float = $(`<div class="floating-quantity">${quantity}</div>`);
        $element.append($float);

        $float.animate({
            top: "-30px", 
            opacity: 0
        }, 500, function () {
            $float.remove();
        });
    }

    if (!sessionStorage.getItem("deliveryNoticeShown")) {
        const $popup = $('<div class="delivery-popup">🎉 Доставка до 2 км — бесплатна!</div>');
        $('body').append($popup);

        setTimeout(() => {
            $popup.addClass('visible');
        }, 100);

        setTimeout(() => {
            $popup.removeClass('visible');
            setTimeout(() => $popup.remove(), 300);
        }, 4000);

        sessionStorage.setItem("deliveryNoticeShown", "true");
    }

    $(".cart-increase").on("click", function () {
        const productId = $(this).data("product-id");
        const $productCard = $(this).closest(".product-card");

        $.ajax({
            url: "/User/AddProductToCart",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(productId),
            success: function (quantity) {
                const $quantitySpan = $productCard.find(".product-quantity");
                $quantitySpan.text(quantity);
                showFloatingQuantity($quantitySpan.parent(), `+1`);
                updateTotalPrice();
            },
            error: function () {
                alert("Error increasing quantity.");
            }
        });
    });

    $(".cart-decrease").on("click", function () {
        const productId = $(this).data("product-id");
        const $productCard = $(this).closest(".product-card");

        $.ajax({
            url: "/User/RemoveProductFromCart",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(productId),
            success: function (quantity) {
                const $quantitySpan = $productCard.find(".product-quantity");
                if (quantity > 0) {
                    $quantitySpan.text(quantity);
                    showFloatingQuantity($quantitySpan.parent(), `-1`);
                } else {
                    $productCard.remove();
                }
                updateTotalPrice();
                checkIfCartIsEmpty();
            },
            error: function () {
                alert("Error decreasing quantity.");
            }
        });
    });

    function checkIfCartIsEmpty() {
        if ($(".product-card").length === 0) {
            $(".cart-layout").hide();
            $(".map-fullwidth-new").hide();
            $(".empty-message").addClass("isZeroForMessage");
        } else {
            $(".cart-layout").show();
            $(".map-fullwidth-new").show();
            $(".empty-message").removeClass("isZeroForMessage");
        }
    }

    checkIfCartIsEmpty();
});