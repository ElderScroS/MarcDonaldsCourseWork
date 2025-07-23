document.addEventListener('DOMContentLoaded', function () {
    const cookieName = 'cookiesAccepted';

    function getCookie(name) {
        const value = "; " + document.cookie;
        const parts = value.split("; " + name + "=");
        if (parts.length === 2) return parts.pop().split(";").shift();
    }

    if (!getCookie(cookieName)) {
        document.getElementById('cookieConsentModal').style.display = 'block';
    }

    document.getElementById('acceptCookiesButton').addEventListener('click', function () {
        var expires = new Date();
        expires.setFullYear(expires.getFullYear() + 1); // кука на 1 год
        document.cookie = cookieName + "=true; expires=" + expires.toUTCString() + "; path=/";
        document.getElementById('cookieConsentModal').style.display = 'none';
    });
});