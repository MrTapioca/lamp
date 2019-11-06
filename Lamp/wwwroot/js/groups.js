$(function () {
    $('[data-leave-button]').click(function (event) {
        var groupId = $(event.target).data('group-id');
        var groupName = $(event.target).data('group-name');

        $('#leave-group-name').text(groupName);
        $('#leave-group-code').val(groupId);

        $('#leave-modal').modal();
    })
})