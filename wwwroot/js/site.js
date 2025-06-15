// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

    document.querySelector(".back-button").addEventListener("click", function () {
        window.history.back();
    });
$(function () {
    $('#editUserForm').on('submit', function (e) {
        e.preventDefault(); // Previne reincarcarea paginii

        // Preluam datele din formular
        var formData = new FormData(this);

        $.ajax({
            url: '/Admin/EditUser', // Asigura-te ca URL-ul este corect
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    // Afiseaza mesajul intr-un modal
                    $('#successModal .modal-body').text(response.message);
                    $('#successModal').modal('show');
                } else {
                    // Afiseaza erorile
                    let errorMessages = response.errors || [response.message];
                    let errorHtml = errorMessages.map(err => `<p>${err}</p>`).join('');
                    $('#ajaxMessage').html(errorHtml).addClass('alert-danger').show();
                }
            },
            error: function () {
                $('#ajaxMessage').html('A aparut o eroare. incercati din nou.').addClass('alert-danger').show();
            }
        });
    });
});

