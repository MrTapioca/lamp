var currentMember;

function startParticipantForm(containerElement) {

    // Initialize selectize input
    $('.participant-form input.form-control', containerElement).selectize({
        labelField: 'name',
        valueField: 'id',
        searchField: 'name',
        maxItems: 1,
        maxOptions: 20,
        closeAfterSelect: true,
        //preload: true,
        options: new Array(currentMember),
        load: function (query, callback) {
            // Make sure there are at least 3 characters to load names
            if (query.trim().length < 3) {
                this.clearOptions();
                this.addOption(currentMember);
                this.refreshOptions();

                return callback();
            }

            var url = '/members/query/' + $('#group').val() + '?query=' + query;
            $.ajax({
                url: url,
                type: 'GET',
                error: function () {
                    callback();
                },
                success: function (data) {
                    callback(data);
                }
            });
        },
        render: {
            option: function (item, escape) {
                return '<div class="participant-list-item">' +
                    '<span class="participant-profile">' + escape(item.initials) + '</span>' +
                    '<span class="participant-profile-name">' + escape(item.name) + '</span>' +
                    '</div>';
            },
            item: function (item, escape) {
                return '<div class="participant-selected">' +
                    '<span class="participant-profile participant-profile-selected">' + escape(item.initials) + '</span>' +
                    '<span class="participant-profile-name">' + escape(item.name) + '</span>' +
                    '</div>';
            }
        }
    });

    // Set up participant form submission
    $('.participant-form button', containerElement).click(function () {
        //var $shift = $(this).closest('.shift-container');
        var $form = $(this).closest('form');
        var shiftId = $form.data('shift-id');
        //var memberId = $form.find('input').val();

        $.ajax({
            url: '/shifts/AddParticipant',
            type: 'POST',
            data: $form.serialize()
        }).done(function () {
            reloadShift(shiftId);
        }).fail(function () {
            alert('Error adding participant');
        });
    });

    // Set up participant removal
    $('.shift-spot button', containerElement).click(function () {
        var name = $(this).siblings('.shift-profile-name').text();
        var participantId = $(this).data('participant-id');
        var shiftId = $(this).closest('.shift-container').data('shift-id');

        $('#confirmation-title').html('Are you sure?');
        $('#confirmation-body').html(`<p>Remove participant <b>${name}</b>?</p>`);
        $('#confirmation-button').text('Yes, remove');
        $('#confirmation-button').off('click');
        $('#confirmation-button').click(function () {
            $.ajax({
                url: '/shifts/RemoveParticipant',
                type: 'POST',
                data: {
                    participantId: participantId,
                    __RequestVerificationToken: $('#confirmation-modal [name="__RequestVerificationToken"]').val()
                }
            }).done(function () {
                reloadShift(shiftId);
            }).fail(function () {
                alert('Error removing participant.');
            });
        });

        $('#confirmation-modal').modal();
    });
}

function startShiftContainers(callback) {

    $.ajax({
        method: 'GET',
        url: '/members/currentuser/' + $('#group').val()
    }).done(function (member) {
        currentMember = member;
        $('.shift-container').each(function (index, element) {
            startParticipantForm(element);
        });
        callback();
    });
}

function reloadShift(shiftId) {
    $.ajax({
        method: 'GET',
        cache: false,
        url: `/shifts/view/${shiftId}`
    }).done(function (newShift) {
        reloadShiftView(shiftId, newShift);
    });
}

function reloadShiftView(shiftId, newView) {
    var $shift = $(`.shift-container[data-shift-id='${shiftId}']`);

    var newContent = $(newView).html();
    //$shift.fadeOut('fast', function () {
        $shift.html(newContent);
        //$shift.fadeIn('fast');
        startParticipantForm($shift);
    //});
}