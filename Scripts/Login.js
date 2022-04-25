function onSignIn(googleUser) {
    var profile = googleUser.getBasicProfile();
    var Email = profile.getEmail();

    /* if (Email.indexOf("fpt.edu.vn") == -1) {
         var auth2 = gapi.auth2.getAuthInstance();
         auth2.signOut();
         document.getElementById("label_Error").style.display = "block";
     } else {*/

    $.ajax({

        url: 'HomeTotal/getInfoUser',

        type: 'POST',
        dataType: 'json',
        data: {

            email: profile.getEmail(),
            name: profile.getName(),
            image_URL: profile.getImageUrl(),

        },
        success: function (response) {
            window.location.href = response.Url;
        },
        error: function () {


        }
    });

   
    /*}*/
    var auth2 = gapi.auth2.getAuthInstance();
    auth2.signOut();
}
function onFailure(error) {

}
function renderButton() {
    gapi.signin2.render('my-signin2', {
        'scope': 'profile email',
        'width': 240,
        'height': 50,
        'longtitle': true,
        'theme': 'dark',
        'onsuccess': onSignIn,
        'onfailure': onFailure
    });
}
function onLoad() {
    gapi.load('auth2', function () {
        gapi.auth2.init();
    });
}

function signOut() {
    var auth2 = gapi.auth2.getAuthInstance();

    auth2.signOut().then(function () {

    });
}





