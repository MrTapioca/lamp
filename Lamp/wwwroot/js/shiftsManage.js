// Show confirmation modal to edit shift
function editShift(shiftId) {
    // Get shift data
    var $shift = $(`.shift-container[data-shift-id='${shiftId}']`);
    var shiftData = $shift.find('[data-shift-data]').data();

    var $modal = $('#create-shift-modal');
    var $form = $modal.find('form');
    var $button = $form.find('button[data-submit]');

    $form.find('[data-enable-shift]').hide();
    $form.find('[data-copies-panel]').hide();

    $modal.find('.modal-title').text(`Edit shift: ${shiftData.start}`);
    $form.find('[data-shift-id]').val(shiftData.shiftId);
    $form.find('[data-picker-start]').data('DateTimePicker').date(shiftData.start);
    $form.find('[data-picker-end]').data('DateTimePicker').date(shiftData.end);
    $form.find('[data-min-participants]').val(shiftData.minParticipants);
    $form.find('[data-max-participants]').val(shiftData.maxParticipants);
    $form.find('[data-instructions]').val(shiftData.instructions);
    $button.text('Update');

    $button.off('click');
    $button.click(function () {

        $form.validate();

        if ($form.valid()) {
            $.ajax({
                url: 'shifts/edit',
                type: 'POST',
                data: $form.serialize()
            }).done(function () {
                reloadShift(shiftData.shiftId);
            });

            $modal.modal('hide');
        }
    });

    $modal.modal();
}

// Enable/disable shift
function enableShift(shiftId, enable) {
    $.ajax({
        url: '/shifts/enable',
        type: 'POST',
        data: {
            shiftId: shiftId,
            enable: enable,
            __RequestVerificationToken: $('#confirmation-modal [name="__RequestVerificationToken"]').val()
        }
    }).done(function () {
        reloadShift(shiftId);
    }).fail(function () {
        alert('Error changing shift.')
    });
}

// Delete shift functionality
function deleteShift(shiftId) {
    var $shift = $(`.shift-container[data-shift-id='${shiftId}']`);
    var shiftData = $shift.find('[data-shift-data]').data();

    $('#confirmation-title').html('Are you sure?');
    $('#confirmation-body').html(`<p>Delete shift <b>${shiftData.start}</b>?</p>`);
    $('#confirmation-button').text('Yes, delete');
    $('#confirmation-button').off('click');
    $('#confirmation-button').click(function () {
        $.ajax({
            url: '/shifts/delete',
            type: 'POST',
            data: {
                shiftId: shiftId,
                __RequestVerificationToken: $('#confirmation-modal [name="__RequestVerificationToken"]').val()
            }
        }).done(function () {
            if ($('.shift-container').length == 1) {
                $('#date-back-button').click();
            } else {
                $shift.fadeOut('fast', function () {
                    $shift.remove();
                })
            }
        }).fail(function () {
            alert('Error deleting shift.');
        });
    });

    $('#confirmation-modal').modal();

}

// Set up copy shift modal
//$(function () {
//    var startEl = $('#copy-shift-modal [data-picker-copy-start]');
//    var endEl = $('#copy-shift-modal [data-picker-copy-through]');

//    $(startEl).datetimepicker({
//        sideBySide: true,
//        ignoreReadonly: true,
//        allowInputToggle: true,
//        useCurrent: false,
//        toolbarPlacement: 'top',
//        showTodayButton: true,
//        showClear: true,
//        showClose: true,
//        stepping: 5
//    });

//    $(endEl).datetimepicker({
//        sideBySide: true,
//        ignoreReadonly: true,
//        allowInputToggle: true,
//        useCurrent: false,
//        toolbarPlacement: 'top',
//        showTodayButton: true,
//        showClear: true,
//        showClose: true,
//        stepping: 5
//    });

//    $(copyUntilEl).datetimepicker({
//        format: 'L',
//        ignoreReadonly: true,
//        allowInputToggle: true,
//        useCurrent: false,
//        toolbarPlacement: 'top',
//        showTodayButton: true,
//        showClear: true,
//        showClose: true,
//    });

//    var pickerStart = $(startEl).data('DateTimePicker');
//    var pickerEnd = $(endEl).data('DateTimePicker');
//    var pickerCopyUntil = $(copyUntilEl).data('DateTimePicker');

//    pickerStart.date(new Date(new Date().setHours(0, 0, 0, 0)));
//    pickerStart.date(null);

//    pickerEnd.date(new Date(new Date().setHours(0, 0, 0, 0)));
//    pickerEnd.date(null);

//    pickerCopyUntil.date(new Date(new Date().setHours(0, 0, 0, 0)));
//    pickerCopyUntil.date(null);

//    // Synchronize pickers
//    $(startEl).on('dp.change', function (e) {
//        if (e.date) {
//            pickerEnd.minDate(e.date);
//            pickerCopyUntil.minDate(e.date.clone().startOf('day'));
//            pickerCopyUntil.maxDate(e.date.clone().add(1, 'years'));
//        } else {
//            pickerEnd.minDate(false);
//            pickerCopyUntil.minDate(false);
//            pickerCopyUntil.maxDate(false);
//        }
//    });
//    //$(endEl).on('dp.change', function (e) {
//    //    if (e.date) {
//    //        var normalized = e.date.seconds(0).milliseconds(0)
//    //        //pickerEnd.date(normalized);
//    //        pickerStart.maxDate(normalized);
//    //    } else {
//    //        pickerStart.maxDate(false);
//    //    }
//    //});

//    // Auto select date on second picker
//    $(startEl).on('dp.hide', function (e) {
//        if (pickerStart.date() !== null &&
//            (pickerEnd.date() === null ||
//                pickerEnd.date() < pickerStart.date())) {
//            pickerEnd.date(e.date);
//        }
//        if (pickerStart.date() !== null &&
//            (pickerCopyUntil.date() === null ||
//                pickerCopyUntil.date() < pickerStart.date().clone().startOf('day'))) {
//            pickerCopyUntil.date(e.date.clone().startOf('day'));
//        }
//    });
//    $(endEl).on('dp.hide', function (e) {
//        if (pickerEnd.date() !== null && pickerStart.date() === null) {
//            pickerStart.date(e.date);
//        }
//    });

//    // Display time difference
//    $(startEl).on('dp.change', function () {
//        displayDuration();
//    });
//    $(endEl).on('dp.change', function () {
//        displayDuration();
//    });

//    function displayDuration() {
//        var start = pickerStart.date();
//        var end = pickerEnd.date();
//        if (start !== null && end !== null) {
//            var diff = end.diff(start, 'hours', true);
//            var word, text;
//            if (diff >= 0) {
//                word = diff == 1 ? 'hour' : 'hours';
//                text = `in ${diff % 1 == 0 ? diff : diff.toFixed(2)} ${word}`;
//            } else {
//                text = '';
//            }
//            $('#create-shift-modal [data-shift-duration]').text(text);
//        } else {
//            $('#create-shift-modal [data-shift-duration]').text('');
//        }
//    }

//    $(copyUntilEl).on('dp.hide', function (e) {
//        if (pickerCopyUntil.date() === null && pickerStart.date() !== null) {
//            pickerCopyUntil.date(pickerStart.date().clone().startOf('day'));
//        }
//    });
//});
