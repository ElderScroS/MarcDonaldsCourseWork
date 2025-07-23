$(document).ready(function () {
    const $modalOverlay = $(".modal-overlay-payment");
    const $closeModalBtn = $(".close-btn-payment");
    const $addCardBtn = $(".add-card-button");
    const $backBtn = $(".back-btn-payment");
    const $modalContainer = $(".modal-container-relative-payment"); 
    const $modalTitle = $('.modal-title-dynamic-payment');
    const $cardNumber = $('#card-number');
    const $expiry = $('#expiry');
    const $cvc = $('#cvc');
    const $brandIcon = $('#card-brand-icon');
    const $paymentsListView = $("#payment-selection");
    const $addCardFormView = $("#add-card");

    let modalPaymentHistory = [];

    function resetModal() {
        const $modal = $(".modal-view-payment");
        const $inputs = $modal.find("input[type='text']");
        const $btn = $(".add-card-btn");

        // Очистить все текстовые поля
        $inputs.val("");

        $modal.removeClass("active slide-in slide-out");

        $modal.find("input[type='checkbox']").prop("checked", false);

        $btn.removeClass("loading").prop("disabled", true);
        $btn.text("Add Payment Method");

        $("#card-brand-icon").attr("src", "/images/other/placeholder.png");

        modalPaymentHistory = [];

        $paymentsListView.addClass("active slide-in");

        $backBtn.hide();

        $modal.find(".error").removeClass("error");
    }

    function openModal() {
        resetModal();
        $modalOverlay.css("display", "flex").removeClass("hide").addClass("show");
        $("body").addClass("modal-open");
        $modalTitle.text('Payment methods');
    }

    function closeModal() {
        $modalOverlay.removeClass("show").addClass("hide");
        setTimeout(() => {
            $modalOverlay.css("display", "none");
            $("body").removeClass("modal-open");
            resetModal();
        }, 300);
    }

    function switchToAddCard() {
        $paymentsListView.removeClass("active slide-in").addClass("slide-out");
        $addCardFormView.removeClass("slide-out").addClass("slide-in active").addClass("addContent");
        $backBtn.show();
        $modalTitle.text('Add card');
        modalPaymentHistory.push("add-card");
    }

    $backBtn.on("click", function () {
        if (modalPaymentHistory.length === 0) return;

        const previous = modalPaymentHistory.pop();
        $(".modal-view-payment").removeClass("active slide-in slide-out");

        if (previous === "add-card") {
            $paymentsListView.addClass("active slide-in");
            modalPaymentHistory = []; 
            $backBtn.hide();
            return;
        }

        if (modalPaymentHistory.length === 0) {
            $backBtn.hide();
        }
    });

    $modalContainer.on('scroll', function () {
        const scrollTop = $modalContainer.scrollTop();

        if (scrollTop > 10) {
            $('.modal-header-payment').addClass('scrolled');
            
            if ($paymentsListView.is(':visible')) {
                $modalTitle.text('Payment methods');
            } else if ($addCardFormView.is(':visible')) {
                $modalTitle.text('Add card');
            }
        } else {
            $('.modal-header-payment').removeClass('scrolled');
            $modalTitle.text('');
        }
    });

    $("#paymentBtn").on("click", openModal);
    $closeModalBtn.on("click", closeModal);
    $modalOverlay.on("click", function (e) {
        if (e.target === this) closeModal();
    });
    $addCardBtn.on("click", switchToAddCard);

    $cardNumber.on('input', function () {
        let value = $(this).val().replace(/\D/g, '').substring(0, 16);
        let formatted = '';
        for (let i = 0; i < value.length; i += 4) {
            if (i > 0) formatted += ' ';
            formatted += value.substring(i, i + 4);
        }
        $(this).val(formatted);

        if (value.length >= 4) {
            const prefix = value.substring(0, 4);
            if (prefix.startsWith('4')) {
                $brandIcon.attr('src', '/images/other/visa.jpg');
            } else {
                const prefixInt = parseInt(prefix, 10);
                if (
                    (prefixInt >= 5100 && prefixInt <= 5599) ||
                    (prefixInt >= 2221 && prefixInt <= 2720)
                ) {
                    $brandIcon.attr('src', '/images/other/mastercard.jpg');
                } else {
                    $brandIcon.attr('src', '/images/other/placeholder.png');
                }
            }
        } else {
            $brandIcon.attr('src', '/images/other/placeholder.png');
        }

        validateCardForm();
    });

    $expiry.on('input', function () {
        let value = $(this).val().replace(/\D/g, '').substring(0, 4);
        if (value.length >= 3) {
            value = value.substring(0, 2) + '/' + value.substring(2);
        }
        $(this).val(value);
        validateCardForm();
    });

    $cvc.on('input', function () {
        let value = $(this).val().replace(/\D/g, '').substring(0, 3);
        $(this).val(value);
        validateCardForm();
    });

    $('#card-number, #expiry, #cvc').on('keypress', function (e) {
        if (!/\d/.test(e.key)) {
            e.preventDefault();
        }
    });

    $('.select-payment-btn').each(function () {
        $(this).on('click', function () {
            const payment = $(this).data('payment');

            $.ajax({
                type: 'POST',
                url: '/User/SelectPayment',
                contentType: "application/json",
                data: JSON.stringify(payment),
                success: function (response) {
                    const $icon = $('.icon-payment');
                    const $text = $('.payment-name');

                    if (payment === "Apple" || payment === "Google") {
                        $icon.removeClass().addClass('fa-solid fa-credit-card icon-payment');
                        $text.text(payment + " Pay");

                        $('.tips-section').removeClass('tips-hide').hide().fadeIn(400);
                    } else {
                        $icon.removeClass().addClass('fa-solid fa-money-bill icon-payment');
                        $text.text("Cash");

                        $('.tips-section').fadeOut(400, function () {
                            $(this).addClass('tips-hide');
                        });

                        $(".tip-btn").removeClass("selected");
                        const $zeroBtn = $(".tip-btn").first();
                        $zeroBtn.addClass("selected");

                        $(".tips-amount").text("0,00 AZN");
                        $(".tips-summary-line").remove();

                        updateTotalPrice();
                    }

                    $('.select-payment-btn').each(function () {
                        if ($(this).data('payment') === payment) {
                            $(this).addClass('disabled').prop('disabled', true);
                        } else {
                            $(this).removeClass('disabled').prop('disabled', false);
                        }
                    });

                    $('.select-btn-card').removeClass('cardSelected').show();
                    closeModal();
                },
                error: function () {}
            });
        });
    });

    function validateCardForm() {
        const cardNumber = $cardNumber.val().replace(/\s/g, '');
        const expiry = $expiry.val();
        const cvc = $cvc.val();

        const isCardNumberValid = cardNumber.length === 16;
        const isExpiryFormatValid = /^\d{2}\/\d{2}$/.test(expiry);
        let isExpiryDateValid = false;

        if (isExpiryFormatValid) {
            const [monthStr, yearStr] = expiry.split('/');
            const month = parseInt(monthStr, 10);
            const year = parseInt(yearStr, 10);

            if (month >= 1 && month <= 12) {
                const currentDate = new Date();
                const currentMonth = currentDate.getMonth() + 1;
                const currentYear = currentDate.getFullYear() % 100;

                if (year > currentYear || (year === currentYear && month >= currentMonth)) {
                    isExpiryDateValid = true;
                }
            }
        }

        const isCvcValid = cvc.length === 3;

        const isFormValid = isCardNumberValid && isExpiryFormatValid && isExpiryDateValid && isCvcValid;

        $('.add-card-btn').prop('disabled', !isFormValid);
    }

    $('.toast-btn').on('click', function () {
        $('#success-toast').removeClass('visible');
    });

    const $btn = $('.add-card-btn');

    $btn.on("click", function () {
        const cardNumber = $('#card-number').val().replace(/\s/g, '');
        const expiry = $('#expiry').val().split('/');
        const cvc = $('#cvc').val();
        const cardName = $('#card-name-input').val();
        const isSelected = $('.switch-default input').is(':checked');

        const cardData = {
            CardNumber: cardNumber,
            ExpiryMonth: expiry[0],
            ExpiryYear: expiry[1],
            CVV: cvc,
            CardName: cardName,
            IsSelected: isSelected
        };

        $btn.addClass('loading-add').prop('disabled', true);

        $.ajax({
            url: '/User/AddCard',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(cardData),
            success: function (response) {
                setTimeout(() => {
                    closeModal();

                    $icon.removeClass().addClass('fa-solid fa-money-bill icon-payment');
                    const $text = $('.payment-name');

                    const $toast = $('#success-toast');
                    $toast.addClass('visible');

                    const first6 = cardNumber.slice(0, 6);
                    const last4 = cardNumber.slice(-4);
                    const masked = `${first6}******${last4}`;

                    if (isSelected) {
                        $('.tips-section').removeClass('tips-hide').hide().fadeIn(200);
                        $('.icon-payment').replaceWith('<i class="fa-solid fa-credit-card icon-payment"></i>');
                        $('.payment-name').text(cardName?.trim() || masked);
                        $('.select-btn-card').removeClass('cardSelected').show();
                    } else {
                        if (payment === "Apple" || payment === "Google") {
                            $icon.removeClass().addClass('fa-solid fa-credit-card icon-payment');
                            $text.text(payment + " Pay");

                            $('.tips-section').removeClass('tips-hide').hide().fadeIn(400);
                        } 
                        else {
                            $(".tip-btn").removeClass("selected");
                            const $zeroBtn = $(".tip-btn").first();
                            $zeroBtn.addClass("selected");

                            $('.tips-section').fadeOut(400, function () {
                                $(this).addClass('tips-hide');
                            });

                            $(".tips-amount").text("0,00 AZN");
                            $(".tips-summary-line").remove();
                        }
                    }

                    const isVisa = cardNumber.startsWith('4'); 
                    const cardNameText = cardName.trim() || "Card";
                    const selectedClass = isSelected ? "cardSelected" : "";

                    const newCardHtml = `
                        <div class="card-item">
                            <div class="img-name">
                                <img class="${isVisa ? 'visa-img' : 'master-img'}" src="/images/other/${isVisa ? 'visa' : 'mastercard'}.jpg" />
                                <div class="name-number">
                                    <p class="card-name">${cardNameText}</p>
                                    <p class="card-number">${masked}</p>
                                </div>
                            </div>
                            <div class="payment-btns">
                                <button class="select-btn-card ${selectedClass}" data-card-id="${response.cardId}" data-card-number="${cardNumber}">
                                    Select
                                </button>
                                <button class="delete-btn-card" data-card-id="${response.cardId}">Delete</button>
                            </div>
                        </div>
                        <hr class="card-hr" />
                    `;

                    $('.card-list-container .add-card-button').before(newCardHtml);

                    setTimeout(() => {
                        $toast.removeClass('visible');
                    }, 5000);
                }, 4000);
            },
            error: function () {
                alert("Server error.");
                $btn.removeClass('loading-add').prop('disabled', false);
            }
        });

        updateTotalPrice();
    });

    $('.delete-btn-card').on("click", function () {
        const $btn = $(this);
        const cardId = $btn.data("card-id");

        if (!cardId) return;
        
        $.ajax({
            url: "/User/DeleteCard",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(cardId),
            success: function (response) {
                window.location.href = "/User/Cart";
            },
            error: function () {
                alert("Произошла ошибка при удалении.");
            }
        });
    });
    
    $('.select-btn-card').on("click", function () {
        const $btn = $(this);
        const cardId = $btn.data("card-id");
        const cardName = $btn.data("card-name");
        const cardNumber = String($btn.data("card-number"));

        const isCurrentlyCash = $('.payment-name').text().trim().toLowerCase() === "cash";

        $.ajax({
            url: '/User/SelectCard',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(cardId),
            success: function () {
                const first6 = cardNumber.slice(0, 6);
                const last4 = cardNumber.slice(-4);
                const masked = `${first6}******${last4}`;

                $('.icon-payment').replaceWith('<i class="fa-solid fa-credit-card icon-payment"></i>');
                if (cardName && cardName.trim() !== "") {
                    $('.payment-name').text(cardName.trim());
                } else {
                    $('.payment-name').text(masked);
                }

                $('.select-btn-card').removeClass('cardSelected');
                $btn.addClass('cardSelected');
                $('.select-btn-card').show();
                $('.select-btn-card.cardSelected').hide();
                $('.select-payment-btn').removeClass('disabled').prop('disabled', false);

                if (isCurrentlyCash) {
                    $('.tips-section').removeClass('tips-hide').hide().fadeIn(200);

                    $(".tip-btn").removeClass("selected");
                    const $zeroBtn = $(".tip-btn").first();
                    $zeroBtn.addClass("selected");

                    $(".tips-amount").text("0,00 AZN");
                    $(".tips-summary-line").remove();

                    updateTotalPrice();
                }

                closeModal();
            },
            error: function () {
                alert("Error occurred while selecting the card.");
            }
        });
    });

    updateTotalPrice();
});
