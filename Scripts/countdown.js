window.onload = function () {
    // Set the date we're counting down to 20s
    var time = document.getElementById("time")

    var value = time.getAttribute('data-time') * 1000;

    var countDownDate = new Date().getTime() + value;

    // Update the count down every 1 second
    var x = setInterval(function () {

        // Get today's date and time
        var now = new Date().getTime();

        // Find the distance between now and the count down date
        var distance = countDownDate - now;

        // Time calculations for days, hours, minutes and seconds
        var days = Math.floor(distance / (1000 * 60 * 60 * 24));
        var hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        var minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60)) + (hours*60);
        var seconds = Math.floor((distance % (1000 * 60)) / 1000);

        // Display the result in the element with id="demo"
        time.innerHTML = format(minutes) + ":" + format(seconds);

        // If the count down is finished, write some text
        if (distance <= 0) {
            clearInterval(x);
            document.getElementById("time").innerHTML = "EXPIRED";
            document.getElementById("btnSubmit").disabled = true;
            document.getElementById("formPaperQuiz").submit();
        }

        if($('#btnSubmit').is(':disabled')){
            clearInterval(x);
            document.getElementById("time").innerHTML = "00:00";
        }

    }, 1000);


    function format(x) {
        if (x < 10) {
            return '0' + x;
        } else {
            return x
        }
    }
};

function submitPaper() {
    var time = document.getElementById("time");
    time.setAttribute('data-time', 0);
    document.getElementById("time").innerHTML = "EXPIRED";
    document.getElementById("btnSubmit").disabled = true;
    document.getElementById("formPaperQuiz").submit();
}