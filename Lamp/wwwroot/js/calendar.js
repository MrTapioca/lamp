var calendar;
var navigatedToPage;

$(function () {
    if (location.hash.startsWith('#')) {

    }

    window.onhashchange = function () {
        if (location.hash === '' && navigatedToPage === true) {
            navigatedToPage = false;
            $('#date-back-button').click();
        }
    }
});

$(function () {

    // Load correct month from the saved cookie
    var calendarStart = Cookies.get('calendarStart');
    if (!moment(calendarStart, 'YYYY-MM-DD').isValid()) {
        calendarStart = moment().format('YYYY-MM-DD');
    }

    // Initialize calendar
    calendar = new FullCalendar.Calendar($('#calendar').get(0), {
        plugins: ['bootstrap', 'dayGrid', 'interaction'],
        themeSystem: 'bootstrap',
        showNonCurrentDates: false,
        fixedWeekCount: false,
        contentHeight: 'auto',
        defaultDate: calendarStart,
        events: '/shifts/events/' + $('#location').val(),
        eventRender: function (info) {
            //info.el.style.backgroundColor = '#d9534f';
            //info.el.style.borderColor = '#d9534f';
            var className;
            switch (info.event.extendedProps.status) {
                case 0:
                    className = 'event-disabled';
                    break;
                case 1:
                    className = 'event-empty';
                    break;
                case 2:
                    className = 'event-incomplete';
                    break;
                case 3:
                    className = 'event-complete';
                    break;
                case 4:
                    className = 'event-full';
                    break;
            }
            info.el.classList.add(className);
        },
        dateClick: function (dateClickInfo) {
            // Check if date clicked has events
            var d = dateClickInfo.date;
            var events = calendar.getEvents();
            for (event of events) {
                var ed = event.start;
                if (ed.getFullYear() === d.getFullYear() &&
                    ed.getMonth() === d.getMonth() &&
                    ed.getDate() === d.getDate()) {

                    showDate(dateClickInfo.date);
                    break;
                }
            }
        },
        eventClick: function (eventClickInfo) {
            showDate(eventClickInfo.event.start);
        },
        datesRender: function (info) {
            // Save month view
            Cookies.set('calendarStart', moment(info.view.currentStart).format('YYYY-MM-DD'), { expires: 1, path: '' });
            $('#calendar').animate({ 'opacity': 1 }, 'slow');
        }
    });

    // Show calendar
    $('#calendar').css('opacity', 0.01);
    calendar.render();
});

// Select group
function setGroup(groupId) {
    Cookies.set('selectedGroup', groupId);
    location.reload();
}

// Select location
function setLocation(locationId) {
    Cookies.set('selectedLocation', locationId);

    // Repopulate calendar with shifts
    calendar.getEventSources()[0].remove();
    calendar.addEventSource('/shifts/events/' + locationId);
}

// Date selection
function showDate(date) {
    var dateString = moment(date).format('YYYY-MM-DD');
    var $calendarPage = $('#calendar-page');
    var $datePage = $('#date-page');
    var $animation = $datePage.find('#date-loading-animation');
    var $content = $datePage.find('#date-content');

    $datePage.find('#date-selected').text(moment(date).format('LL'));
    $content.hide().empty();
    $animation.show();

    var url = `/shifts/view/${$('#location').val()}/${dateString}`;
    $.get(url, function (data) {
        $content.html(data);

        navigatedToPage = true;
        location.hash = dateString;

        startShiftContainers(function () {
            $animation.fadeOut('fast', function () {
                $content.fadeIn('fast');
            });
        });
    });

    $calendarPage.fadeOut('fast', function () {
        $datePage.fadeIn('fast');
    });
}

// Date back button
$('#date-back-button').click(function () {
    $('#date-page').fadeOut('fast', function () {
        if (navigatedToPage === true) {
            navigatedToPage = false;
            window.history.back();
        }
        $('#calendar-page').fadeIn('fast');
        calendar.refetchEvents();
    });
});