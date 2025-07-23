$(document).ready(function () {
    const addressSpanText = $("span:contains('None')").length > 0 ? "None" : "Exist";
    const userHasAddress = addressSpanText !== "None";

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

    $(".add-to-cart-btn").on("click", function () {
        if (!userHasAddress) {
            const $modal = $("#addressModal");
            $("body").addClass("no-scroll");
            $modal.removeClass("d-none fade-out").addClass("fade-in");
            return;
        }

        const productId = $(this).data("product-id");
        const $productCard = $(this).closest(".product");

        $.ajax({
            url: "/User/AddProductToCart",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(productId),
            success: function (quantity) {
                $productCard.find(".add-to-cart-btn").addClass("d-none");
                $productCard.find(".quantity-controls").removeClass("d-none");
                $productCard.find(".product-quantity").text(quantity);
            },
            error: function () {
                alert("Error adding product to cart.");
            }
        });
    });

    $("#closeModalBtn").on("click", function () {
        const $modal = $("#addressModal");
        $modal.removeClass("fade-in").addClass("fade-out");

        setTimeout(() => {
            $modal.addClass("d-none");
            $("body").removeClass("no-scroll");
        }, 400);
    });

    $(".cart-increase").on("click", function () {
        const productId = $(this).data("product-id");
        const $productCard = $(this).closest(".product");

        $.ajax({
            url: "/User/AddProductToCart",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(productId),
            success: function (quantity) {
                const $quantitySpan = $productCard.find(".product-quantity");
                $quantitySpan.text(quantity);
                showFloatingQuantity($quantitySpan.parent(), `+1`);
            },
            error: function () {
                alert("Error increasing quantity.");
            }
        });
    });

    $(".cart-decrease").on("click", function () {
        const productId = $(this).data("product-id");
        const $productCard = $(this).closest(".product");

        $.ajax({
            url: "/User/RemoveProductFromCart",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(productId),
            success: function (quantity) {
                if (quantity > 0) {
                    const $quantitySpan = $productCard.find(".product-quantity");
                    $quantitySpan.text(quantity);
                    showFloatingQuantity($quantitySpan.parent(), `-1`);
                } else {
                    $productCard.find(".quantity-controls").addClass("d-none");
                    $productCard.find(".add-to-cart-btn").removeClass("d-none");
                }
            },
            error: function () {
                alert("Error decreasing quantity.");
            }
        });
    });
});