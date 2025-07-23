$('#openDeleteModal').on('click', function (e) {
    e.preventDefault();
    const modal = $('#deleteModal');

    modal.removeClass('fade-out').addClass('fade-in').css('display', 'flex');
    $('body').addClass('modal-open');
});

function closeModal() {
    const modal = $('#deleteModal');

    modal.removeClass('fade-in').addClass('fade-out');

    setTimeout(() => {
        modal.css('display', 'none');
        $('body').removeClass('modal-open');
    }, 300);
}

$('#closeModalBtn').on('click', closeModal);

$(window).on('click', function (e) {
    if ($(e.target).is('#deleteModal')) {
        closeModal();
    }
});
