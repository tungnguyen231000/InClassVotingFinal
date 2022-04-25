$(document).ready(function () {

    

    var indexItem = 0;
    var listTime = document.querySelectorAll("#time");
    var checkPreview = false;
    if ($(".divs").attr("data-model") == "preview") {
        checkPreview = true;
    }
    if (!checkPreview) {
        timeCountDown(0);
    }
    
    if (indexItem == 0) {
        $("#prev").hide();
    } else {
        $("#prev").show();
    }

    $(".divs > .cls").each(function (e) {
        if (e != 0)
            $(this).hide();
    });

    if (listTime.length == 1) {
        $("#next").val("Submit");
        if (checkPreview) {
            $("#next").prop("disabled", true);
        }
    }

    $("#next").click(function (e) {
        if ($(".divs > .cls:visible").next().length != 0) {
            $(".divs > .cls:visible").next().show().prev().hide();

            if (indexItem == (listTime.length - 2)) {
                ++indexItem;
                if (!checkPreview) {
                    timeCountDown(indexItem);
                }
                $("#next").val("Submit");
                if (checkPreview) {
                    $("#next").prop("disabled", true);
                }

            } else {
                ++indexItem;
                if (!checkPreview) {
                    timeCountDown(indexItem);
                }
                $("#prev").show();

                if (checkPreview) {
                    $("#next").prop("disabled", false);
                }
            }
        }
        else {
            
            $("#orderTest").submit();
        }
        e.preventDefault();

        return false;
    });

    $("#prev").click(function (e) {
        if ($(".divs > .cls:visible").prev().length != 0) {
            $(".divs > .cls:visible").prev().show().next().hide();
            --indexItem;
            if (!checkPreview) {
                timeCountDown(indexItem);
            }
            $("#next").val("Next");

            if (indexItem == 0) {
                $("#prev").hide();
            } else {
                $("#prev").show();
            }

            if (checkPreview) {
                $("#next").prop("disabled", false);
            }

        }
        else {
            $(".divs > .cls:visible").hide();
            $(".divs > .cls:last").show();
        }

        e.preventDefault();
        return false;
    });



    function timeCountDown(element) {
        var time = listTime[element];
        var value = time.getAttribute('data-time') * 1000;

        var countDownDate = new Date().getTime() + value;

        // Update the count down every 1 second
        var z = setInterval(function () {

            // Get today's date and time
            var now = new Date().getTime();

            // Find the distance between now and the count down date
            var distance = countDownDate - now;

            // Time calculations for days, hours, minutes and seconds
            var days = Math.floor(distance / (1000 * 60 * 60 * 24));
            var hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60)) + (days * 24);
            var minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60)) + (hours * 60);
            var seconds = Math.floor((distance % (1000 * 60)) / 1000);

            // Display the result in the element with id="demo"
            time.innerHTML = format(minutes) + ":" + format(seconds);

            // If the count down is finished, write some text
            if (distance <= 0) {
                clearInterval(z);
                $('#next').click();
            }
        }, 1000);


        $('#next').mousedown(function () {
            clearInterval(z);
        });
        $('#prev').mousedown(function () {
            clearInterval(z);
        });
    }

    function format(x) {
        if (x < 10) {
            return '0' + x;
        } else {
            return x
        }
    }
    
});