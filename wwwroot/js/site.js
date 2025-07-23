document.addEventListener("DOMContentLoaded", function () {
    const filterItems = document.querySelectorAll('.filter-list .flist-item');
    const products = document.querySelectorAll('.products .product');

    filterItems.forEach(item => {
        item.addEventListener('click', function () {

            filterItems.forEach(item => item.classList.remove('active'));
            this.classList.add('active');

            const category = this.getAttribute('data-category');

            products.forEach(product => {
                const productCategory = product.classList.contains(category) || category === 'All';
                if (productCategory) {
                    product.style.display = 'flex';
                } else {
                    product.style.display = 'none';
                }
            });
        });
    });
});

$(document).ready(function () {
    const observer = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                console.log(entry.target); 

                $(entry.target).addClass("show"); 
                observer.unobserve(entry.target);
            }
        });
    }, {
        threshold: 0.05
    });

    $(".fade-in").each(function () {
        observer.observe(this);
    });

    $(".scroll-link").click(function (event) {
        event.preventDefault();
        var target = $(this).attr("href");

        $("html, body").animate({
            scrollTop: $(target).offset().top
        }, 0);
    });
});