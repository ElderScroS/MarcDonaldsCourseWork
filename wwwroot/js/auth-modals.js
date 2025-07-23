$(document).ready(function () {
    function openModalWindow(modalToShow) {
        $(".modal").hide(); 
        $(modalToShow).show();
        $("body").addClass("modal-open");
    }

    function closeModalWindow(modal) {
        $(modal).hide();
        $("body").removeClass("modal-open");
    }

    // Открытие модальных окон
    $("#openModal, #signinLink").on("click", function () {
        openModalWindow("#modalSignin");
    });

    $("#signupLinkFromForgotPassword, #signupLinkFromSignin").on("click", function () {
        openModalWindow("#modalSignup");
    });

    $("#forgotPasswordLinkFromSignin, #forgotPasswordLinkFromSignup").on("click", function () {
        openModalWindow("#modalForgotPassword");
    });

    // Закрытие модальных окон
    $("#closeModalSignin").on("click", function () {
        closeModalWindow("#modalSignin");
    });

    $("#closeModalSignup").on("click", function () {
        closeModalWindow("#modalSignup");
    });

    $("#closeModalForgotPassword").on("click", function () {
        closeModalWindow("#modalForgotPassword");
    });

    // Функция для дрожания кнопки
    function shake(any) {
        any.addClass("shake");
        setTimeout(() => {
            any.removeClass("shake");
        }, 500);
    }

    // #####################        SIGN IN CHECK FORM       #####################
    let signinEmailInput = $("#signinEmailInput");
    let signinUserExistingHint = $("#signinUserExistingHint");
    let signinPasswordInput = $("#signinPasswordInput");
    let signinSubmit = $("#signinSubmit");

    signinSubmit.on("click", function (e) {
        e.preventDefault();
    
        let email = signinEmailInput.val().trim();
        let password = signinPasswordInput.val().trim();
    
        signinEmailInput.removeClass("input-error");
        signinPasswordInput.removeClass("input-error");
        signinUserExistingHint.text("");

        if (!email.trim()) {
            shake(signinEmailInput);
            signinEmailInput.addClass("input-error");
            return;
        }

        if (password.length <= 4) {
            shake(signinPasswordInput);
            signinPasswordInput.addClass("input-error");
            return;
        } else {
            signinPasswordInput.removeClass("input-error");
        }

        const loginRequest = {
            email: email,
            password: password
        };
        
        $.ajax({
            url: "/Auth/CheckUserExists",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(loginRequest),
            success: function (response) {
                if (response === true) {
                    $("#signinForm").submit();
                } else {
                    signinUserExistingHint.text("Such user does not exist!").css("color", "red");
                    shake(signinSubmit);
                }
            },
            error: function (xhr, status, error) {
                shake(signinSubmit);
            }
        });
    });

    // #####################        SIGN UP CHECK FORM       #####################
    let signupEmailInput = $("#signupEmailInput");
    let signupEmailHint = $("#signupEmailHint");
    let signupPasswordInput = $("#signupPasswordInput");
    let signupPasswordHint = $("#signupPasswordHint");
    let signupSubmit = $("#signupSubmit");

    signupPasswordInput.on("input", function () {
        let password = $(this).val().trim();
        const hasDigit = /\d/.test(password);
        const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(password);

        if (password.length >= 7 && hasDigit && hasSpecialChar) {
            signupPasswordHint.text("Password is strong").css("color", "rgb(44, 196, 44)");
        } else {
            signupPasswordHint.text(password === "" ? "At least 7 letters, 1 character, and 1 digit" : "Password is weak").css("color", "red");
        }
    });

    signupSubmit.on("click", function (e) {
        e.preventDefault();
    
        let email = signupEmailInput.val().trim();
        let password = signupPasswordInput.val().trim();
    
        signupEmailInput.removeClass("input-error");
        signupPasswordInput.removeClass("input-error");
    
        if (!email.includes("@") || email.indexOf(".", email.indexOf("@")) === -1) {
            shake(signupEmailInput);
            signupEmailInput.addClass("input-error");
            return;
        }
    
        $.ajax({
            url: "/Auth/CheckEmailExists",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(email),
            success: function (response) {
                if (response === true) {
                    signupEmailInput.addClass("input-error");
                    signupEmailHint.text("This email is already registered!").css("color", "red");
                    shake(signupEmailInput);
                    return;
                }
    
                const hasDigit = /\d/.test(password);
                const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(password);
    
                if (password.length < 7 || !hasDigit || !hasSpecialChar) {
                    shake(signupPasswordInput);
                    signupPasswordInput.addClass("input-error");
                    return;
                }
    
                signupEmailHint.text("");
                $("#signupForm").submit();
            },
            error: function (xhr, status, error) {
                console.error("AJAX Error:", status, error);
                shake(signupSubmit);
            }
        });
    });
    

    // #####################        FORGOT PASSWORD CHECK FORM       #####################
    let forgotPasswordEmailInput = $("#forgotPasswordEmailInput");
    let forgotPasswordEmailHint = $("#forgotPasswordEmailHint");
    let forgotPasswordSubmit = $("#forgotPasswordSubmit");

    forgotPasswordSubmit.on("click", function (e) {
        e.preventDefault();
    
        let email = forgotPasswordEmailInput.val().trim();
    
        forgotPasswordEmailInput.removeClass("input-error");
    
        if (!email.includes("@") || email.indexOf(".", email.indexOf("@")) === -1) {
            shake(forgotPasswordEmailInput);
            forgotPasswordEmailInput.addClass("input-error");
            return;
        }
    
        $.ajax({
            url: "/Auth/CheckEmailExists",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(email),
            success: function (response) {
                if (response === false) {
                    forgotPasswordEmailInput.addClass("input-error");
                    forgotPasswordEmailHint.text("Such email does not exist!").css("color", "red");
                    shake(forgotPasswordEmailInput);
                }
                else {
                    forgotPasswordEmailInput.removeClass("input-error");
                    forgotPasswordEmailHint.text("");
                    $("#forgotPasswordForm").submit();
                }
            },
            error: function (xhr, status, error) {
                console.error("AJAX Error:", status, error);
                shake(forgotPasswordSubmit);
            }
        });
    });
});
