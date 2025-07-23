function checkForm() {
    const inputs = document.querySelectorAll('.form-control');
    let allFilled = true;

    inputs.forEach(input => {
        if (input.type !== 'file' && input.value.trim() === '') {
            allFilled = false;
        }
    });

    const submitButton = document.querySelector('.btn-submit');
    if (allFilled) {
        submitButton.style.opacity = '1';
        submitButton.style.pointerEvents = 'auto';
    } else {
        submitButton.style.opacity = '0.5';
        submitButton.style.pointerEvents = 'none';
    }
}

function previewImage(event) {
    const imagePreview = document.getElementById('imagePreview');
    imagePreview.src = URL.createObjectURL(event.target.files[0]);
    imagePreview.onload = function () {
        URL.revokeObjectURL(imagePreview.src);
    }
}

document.addEventListener('DOMContentLoaded', () => {
    checkForm();
    const inputs = document.querySelectorAll('.form-control');

    inputs.forEach(input => {
        input.addEventListener('input', checkForm);
    });

    const cancelButton = document.getElementById('cancel-button');
    const saveButton = document.getElementById('save-button');

    cancelButton.addEventListener('click', function (e) {
        const cancelInput = document.createElement('input');
        cancelInput.setAttribute('type', 'hidden');
        cancelInput.setAttribute('name', 'cancelBtn');
        cancelInput.setAttribute('value', 'cancelBtn');
        document.forms[0].appendChild(cancelInput);
    });

    saveButton.addEventListener('click', function (e) {
        const cancelInput = document.querySelector('input[name="cancelBtn"]');
        if (cancelInput) {
            cancelInput.remove();
        }
    });
});
