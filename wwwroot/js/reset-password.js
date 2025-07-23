document.addEventListener('DOMContentLoaded', function () {
    const passwordInput = document.querySelector('#password');
    const confirmPasswordInput = document.querySelector('#confirmPassword');
    const resetButton = document.querySelector('#resetButton');
    const passwordHint = document.querySelector('.password-hint');
    const matchHint = document.querySelector('.match-hint');

    resetButton.disabled = true;
    resetButton.style.opacity = '0.5';

    function validatePasswordStrength(password) {
        const hasDigit = /\d/.test(password);
        const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(password);
        return password.length >= 7 && hasDigit && hasSpecialChar;
    }

    function checkPasswords() {
        const password = passwordInput.value;
        const confirmPassword = confirmPasswordInput.value;

        // �������� ��������� ������
        if (password.length === 0) {
            passwordHint.textContent = 'Password must be at least 7 characters long and include at least 1 digit and 1 special character.';
            passwordHint.style.color = '#a62d24';
        } else if (validatePasswordStrength(password)) {
            passwordHint.textContent = 'Password is strong';
            passwordHint.style.color = 'rgba(0, 255, 0, 0.7)';
        } else {
            passwordHint.textContent = 'Password is weak';
            passwordHint.style.color = '#a62d24';
        }

        if (password === confirmPassword && password.length > 0) {
            matchHint.textContent = 'Passwords match';
            matchHint.style.color = 'rgba(0, 255, 0, 0.7)';
        } else {
            matchHint.textContent = 'Passwords do not match';
            matchHint.style.color = '#a62d24';
        }

        if (validatePasswordStrength(password) && password === confirmPassword) {
            resetButton.disabled = false;
            resetButton.style.opacity = '1';
        } else {
            resetButton.disabled = true;
            resetButton.style.opacity = '0.5';
        }
    }

    passwordInput.addEventListener('input', checkPasswords);
    confirmPasswordInput.addEventListener('input', checkPasswords);
});
