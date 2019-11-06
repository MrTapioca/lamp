function copyToClipboard(elementId) {
    var $temp = $("<input>");
    $("body").append($temp);
    $temp.val($(elementId).text()).select();
    document.execCommand("copy");
    $temp.remove();
}

function confirmRemoveLocation(locationId, locationName) {
    $('#remove-location-modal [data-location-name]').text(locationName);
    $('#remove-location-modal [data-location-id]').val(locationId);
    $('#remove-location-modal').modal();
}

function confirmRemove(memberId, memberName) {
    $('#remove-modal [data-member-name]').text(memberName);
    $('#remove-modal [data-member-id]').val(memberId);
    $('#remove-modal').modal();
}

function confirmReject(memberId, memberName) {
    $('#reject-modal [data-member-name]').text(memberName);
    $('#reject-modal [data-member-id]').val(memberId);
    $('#reject-modal').modal();
}

function changeRole(memberId, memberName, newRole, form) {
    var url = form.action;
    var token = $(form).find('input[name="__RequestVerificationToken"]').val();
    $.ajax({
        method: 'POST',
        url: url,
        data: { memberId: memberId, memberName: memberName, newRole: newRole },
        headers: { RequestVerificationToken: token },
        success: function (data) {
            if (data.success === false) {
                showModalAlert(data.message, data.type, true);
            }
        },
        error: function () {
            showModalAlert('Server could not be reached!', 'danger', true);
        }
    });
}