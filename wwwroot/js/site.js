// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
const toogleTr = '.toogle-tr[data-toggle="toggle"]'

$(function () {
    $(toogleTr).on('change', function () {
        $(this).parents().next('.hide').toggle();
        let element = $(this)
        let elementId = element.prop('id')
        if (element.prop('checked')) {
            localStorage.setItem(elementId, 'true')
        } else {
            if (localStorage.getItem(elementId)) localStorage.removeItem(elementId)
        }
    })
    
    // Fetch all the forms we want to apply custom Bootstrap validation styles to
    var forms = document.getElementsByClassName('needs-validation');
    // Loop over them and prevent submission
    Array.prototype.filter.call(forms, function(form) {
        form.addEventListener('submit', function(event) {
            if (form.checkValidity() === false) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });
    $.each($(toogleTr), function (index, value) {
        let element = $(value)
        let elementId = element.prop('id')
        if (localStorage.getItem(elementId)){
            element.prop('checked', true)
            element.trigger("change");
        }
    })
    if (localStorage.getItem("scroll")) {
        window.scrollTo(localStorage.getItem("scroll"), 0)
        console.log(localStorage.getItem("scroll"))
    }

    $('[data-toggle="tooltip"]').tooltip()
    
    if (localStorage.getItem(saveCurrentPageName)){
        document.getElementById('currentPage').value = localStorage.getItem(saveCurrentPageName)
    }
    
    document.addEventListener("reloadend", () => {
        $('[data-toggle="tooltip"]').tooltip()
        if (document.getElementById('listCount') && document.getElementById('mvcGridCount')) {
            $("#listCount").html($("#mvcGridCount").html())
        }
        if (document.getElementById("dactCount")) {
            $("#listCount").html($("#dactCount").html())
        }
        if (document.getElementById("saveCurrentPage") && document.getElementById("currentPage")) {
            let select = document.getElementById("saveCurrentPage")
            if (saveCurrentPage) localStorage.setItem(saveCurrentPageName, select.value)
            document.getElementById('currentPage').value = select.value
        }
    });
    reloadTable()
})