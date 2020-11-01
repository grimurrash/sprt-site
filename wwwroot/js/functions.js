/*

 */
jQuery.fn.exists = function() {
    return $(this).length;
}

function reloadTable() {
    if (!$('.mvc-grid:not(.not-reload)').exists()) return false;
    var grid = new MvcGrid(document.querySelector('.mvc-grid'))
    grid.url.searchParams.set('exitMode', 'false')
    let select;
    if (document.getElementById('DirectiveTypeIdFilter')) {
        select = document.getElementById('DirectiveTypeIdFilter')
        grid.url.searchParams.set('directorTypeId', select.value)
    }

    if (document.getElementById('RequirementTypeIdFilter')) {
        select = document.getElementById('RequirementTypeIdFilter')
        grid.url.searchParams.set('requirementTypeId', select.value)
    }
    
    if (document.getElementById('MilitaryUnitIdFilter')) {
        select = document.getElementById('MilitaryUnitIdFilter')
        grid.url.searchParams.set('militaryUnitId', select.value)
    }
   
    if (document.getElementById('MilitaryComissariatIdFilter')) {
        select = document.getElementById('MilitaryComissariatIdFilter')
        grid.url.searchParams.set('militaryComissariatId', select.value)
    }
    
    if (document.getElementById('Search')) {
        select = document.getElementById('Search')
        grid.url.searchParams.set('search', select.value)
    }
    
    if (document.getElementById('IsMark')) {
        select = document.getElementById('IsMark')
        grid.url.searchParams.set('isMark', select.checked)
    }
    
    grid.url.searchParams.set('page', '1')
    grid.reload();
}


function showModal(e) {
    e.preventDefault()
    let element = e.currentTarget;
    let url = $(element).prop('href')
    $("#modalContainer").load(url, function (responseText, textStatus) {
        if (textStatus === "error") {
            alert($(element).data('alert-text'))
        } else {
            let modalElement = $('#showModal')
            modalElement.modal('toggle');
            modalElement.on('hidden.bs.modal', function () {
                if (!modalElement.data('not-reload')) {
                    reloadTable()
                }
                $("#modalContainer").html("")
            })
        }
    })
}

function actionWarning (e) {
    e.preventDefault();
    let element = $(e.currentTarget)
    if(confirm(element.data('alert-text'))) {
        window.location.href = element.prop('href')
    }
}
function saveModalForm(e) {
    e.preventDefault()
    let form = $(e.currentTarget)
    let url = form.prop('action')
    $.ajax({
        type: 'post',
        url: url,
        data: form.serialize(),
        success: function(data) {
            $('.invalid-feedback.feedback-text').remove();
            $(".is-invalid").removeClass('is-invalid')
            if (data.isSucceeded) {
                $("#showModal").modal('toggle')
            } else {
                $("#modalBody").scrollTop(0);
                $.each(data.errors, function(elementId, value) {
                    let element = $(`#${elementId}`)
                    element.addClass('is-invalid')
                    element.parent().append("<div class='invalid-feedback feedback-text'>" + value[0] + "</div>");
                })
            }
        },
        error: function() {
            alert('Не удалось сохранить изменения')
        }
    })
}
