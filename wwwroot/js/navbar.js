$(document).ready(function () {
    /* DROPDOWN MENU */
    const $button = $('#dropdownButton');
    const $menu = $('#dropdownMenu');

    $button.on('click', function (e) {
        e.stopPropagation();
        $(this).toggleClass('open');
        $menu.toggleClass('show');
    });

    // Клик вне меню — закрыть
    $(document).on('click', function (e) {
        if (!$menu.is(e.target) && $menu.has(e.target).length === 0 &&
            !$button.is(e.target) && $button.has(e.target).length === 0) {
            $menu.removeClass('show');
            $button.removeClass('open');
        }
    });

    /* SCROLLING */

    const $header = $('.header_section');
    let lastScrollTop = 0;

    $(window).on('scroll', function () {
        const currentScroll = $(this).scrollTop();

        if (currentScroll > 10) {
            $header.addClass('header-scrolled').removeClass('header-line');
        } else {
            $header.removeClass('header-scrolled').addClass('header-line');
        }

        if (currentScroll > lastScrollTop && currentScroll > 10) {
            $header.addClass('header-hidden');
        } else {
            $header.removeClass('header-hidden');
        }

        lastScrollTop = currentScroll <= 0 ? 0 : currentScroll;
    });

    // Эффект печатания текста
    const $typingSpan = $('#typingText');
    const fullText = $typingSpan.text();
    const threeMinutes = 3 * 60 * 1000;
    const lastTyped = localStorage.getItem("lastTyped");
    const now = Date.now();

    if (!lastTyped || now - lastTyped >= threeMinutes) {
        $typingSpan.text('');
        let i = 0;

        const typeInterval = setInterval(() => {
            $typingSpan.append(fullText.charAt(i));
            i++;
            if (i >= fullText.length) {
                clearInterval(typeInterval);
                localStorage.setItem("lastTyped", Date.now());
            }
        }, 150);
    }

    $("#languageBtn").on("click", function () {
        $("#language").toggleClass('show');
    });

    $("#language li").on("click", function () {
        const selectedCulture = $(this).data("culture"); 
        const returnUrl = window.location.pathname; // Получаем текущий путь страницы
        const url = `/Culture/SetCulture?culture=${selectedCulture}&returnUrl=${returnUrl}`; // Формируем URL для редиректа

        window.location.href = url;
    });

    $(document).on("click", function (e) {
        if (!$(e.target).closest("#languageBtn, #language").length) {
            $("#language").removeClass('show');
        }
    });

});