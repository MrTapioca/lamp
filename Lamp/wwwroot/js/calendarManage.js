// Create modal setup
function createShift() {
    var $modal = $('#create-shift-modal');
    var $form = $modal.find('form');
    var pickerStart = $form.find('[data-picker-start]').data('DateTimePicker');
    var pickerEnd = $form.find('[data-picker-end]').data('DateTimePicker');
    var pickerCopyUntil = $form.find('[data-picker-copy-until]').data('DateTimePicker');
    var $button = $form.find('button[data-submit]');

    $modal.find('.modal-title').text('Create new shift');
    $form.find('[data-location-id]').val($('#location').val());
    pickerStart.viewDate(calendar.view.currentStart);
    pickerEnd.viewDate(calendar.view.currentStart);
    pickerCopyUntil.viewDate(calendar.view.currentStart);
    $button.text('Create');

    $button.off('click');
    $button.click(function () {

        $form.validate();

        if ($form.valid()) {
            $.ajax({
                url: 'shifts/create',
                type: 'POST',
                data: $form.serialize()
            }).done(function () {
                calendar.refetchEvents();
            });

            $modal.modal('hide');
        }
    });

    $modal.modal();
}

// Reset form when modal closes
$(function () {
    var $modal = $('#create-shift-modal');

    $modal.on('hidden.bs.modal', function () {
        var $form = $modal.find('form');

        $form.clearForm();
        $form[0].reset();
        $form.find('[data-location-id]').val('0');
        $form.find('[data-shift-id]').val('0');

        $form.find('[data-enable-shift]').show();
        $form.find('[data-copies-panel]').show();
        $form.find('[data-copies-toggle]').bootstrapToggle('off');

        var pickerStart = $form.find('[data-picker-start]').data('DateTimePicker');
        pickerStart.date(new Date(new Date().setHours(0, 0, 0, 0)));
        pickerStart.date(null);

        var pickerEnd = $form.find('[data-picker-end]').data('DateTimePicker');
        pickerEnd.date(new Date(new Date().setHours(0, 0, 0, 0)));
        pickerEnd.date(null);

        var pickerCopyUntil = $form.find('[data-picker-copy-until]').data('DateTimePicker');
        pickerCopyUntil.date(new Date(new Date().setHours(0, 0, 0, 0)));
        pickerCopyUntil.date(null);
    });
});

// Date picker configuration
$(function () {
    var startEl = $('#create-shift-modal [data-picker-start]');
    var endEl = $('#create-shift-modal [data-picker-end]');
    var copyUntilEl = $('#create-shift-modal [data-picker-copy-until]');

    $(startEl).datetimepicker({
        sideBySide: true,
        ignoreReadonly: true,
        allowInputToggle: true,
        useCurrent: false,
        toolbarPlacement: 'top',
        showTodayButton: true,
        showClear: true,
        showClose: true,
        stepping: 5
    });

    $(endEl).datetimepicker({
        sideBySide: true,
        ignoreReadonly: true,
        allowInputToggle: true,
        useCurrent: false,
        toolbarPlacement: 'top',
        showTodayButton: true,
        showClear: true,
        showClose: true,
        stepping: 5
    });

    $(copyUntilEl).datetimepicker({
        format: 'L',
        ignoreReadonly: true,
        allowInputToggle: true,
        useCurrent: false,
        toolbarPlacement: 'top',
        showTodayButton: true,
        showClear: true,
        showClose: true,
    });

    var pickerStart = $(startEl).data('DateTimePicker');
    var pickerEnd = $(endEl).data('DateTimePicker');
    var pickerCopyUntil = $(copyUntilEl).data('DateTimePicker');

    pickerStart.date(new Date(new Date().setHours(0, 0, 0, 0)));
    pickerStart.date(null);

    pickerEnd.date(new Date(new Date().setHours(0, 0, 0, 0)));
    pickerEnd.date(null);

    pickerCopyUntil.date(new Date(new Date().setHours(0, 0, 0, 0)));
    pickerCopyUntil.date(null);

    // Synchronize pickers
    $(startEl).on('dp.change', function (e) {
        if (e.date) {
            pickerEnd.minDate(e.date);
            pickerCopyUntil.minDate(e.date.clone().startOf('day'));
            pickerCopyUntil.maxDate(e.date.clone().add(1, 'years'));
        } else {
            pickerEnd.minDate(false);
            pickerCopyUntil.minDate(false);
            pickerCopyUntil.maxDate(false);
        }
    });
    //$(endEl).on('dp.change', function (e) {
    //    if (e.date) {
    //        var normalized = e.date.seconds(0).milliseconds(0)
    //        //pickerEnd.date(normalized);
    //        pickerStart.maxDate(normalized);
    //    } else {
    //        pickerStart.maxDate(false);
    //    }
    //});

    // Auto select date on second picker
    $(startEl).on('dp.hide', function (e) {
        if (pickerStart.date() !== null &&
            (pickerEnd.date() === null ||
                pickerEnd.date() < pickerStart.date())) {
            pickerEnd.date(e.date);
        }
        if (pickerStart.date() !== null &&
            (pickerCopyUntil.date() === null ||
                pickerCopyUntil.date() < pickerStart.date().clone().startOf('day'))) {
            pickerCopyUntil.date(e.date.clone().startOf('day'));
        }
    });
    $(endEl).on('dp.hide', function (e) {
        if (pickerEnd.date() !== null && pickerStart.date() === null) {
            pickerStart.date(e.date);
        }
    });

    // Display time difference
    $(startEl).on('dp.change', function () {
        displayDuration();
    });
    $(endEl).on('dp.change', function () {
        displayDuration();
    });

    function displayDuration() {
        var start = pickerStart.date();
        var end = pickerEnd.date();
        if (start !== null && end !== null) {
            var diff = end.diff(start, 'hours', true);
            var word, text;
            if (diff >= 0) {
                word = diff == 1 ? 'hour' : 'hours';
                text = `in ${diff % 1 == 0 ? diff : diff.toFixed(2)} ${word}`;
            } else {
                text = '';
            }
            $('#create-shift-modal [data-shift-duration]').text(text);
        } else {
            $('#create-shift-modal [data-shift-duration]').text('');
        }
    }

    $(copyUntilEl).on('dp.hide', function (e) {
        if (pickerCopyUntil.date() === null && pickerStart.date() !== null) {
            pickerCopyUntil.date(pickerStart.date().clone().startOf('day'));
        }
    });

    // Auto select min or max date on error
    //$(startEl).on('dp.error', function (e) {
    //    var maxDate = pickerEnd.maxDate();
    //    if (maxDate) {
    //        pickerStart.date(maxDate);
    //    }
    //});

    //$(endEl).on('dp.error', function (e) {
    //    var minDate = pickerEnd.minDate();
    //    if (minDate) {
    //        pickerEnd.date(minDate);
    //    }
    //});
});

// Number input spinner
$(document).on('click', '.number-spinner button', function () {
    var btn = $(this),
        oldValue = btn.closest('.number-spinner').find('input').val().trim(),
        newVal = 0;

    if (btn.attr('data-dir') == 'up') {
        if (oldValue >= 1) {
            newVal = parseInt(oldValue) + 1;
        } else {
            newVal = 1;
        }
    } else {
        if (oldValue > 1) {
            newVal = parseInt(oldValue) - 1;
        } else {
            newVal = 1;
        }
    }
    btn.closest('.number-spinner').find('input').val(newVal);
    btn.closest('.number-spinner').find('input').blur();
});

// Set up copy panel toggle
$(function () {
    $('#create-shift-modal [data-copies-toggle]').change(function () {
        $('#create-shift-modal [data-copies-panel-body]')
            .collapse(this.checked ? 'show' : 'hide');
    });
});