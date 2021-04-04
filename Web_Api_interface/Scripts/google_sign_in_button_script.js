'use strict';
var id_token = "";

function onSignIn(googleUser) {
    //console.log(googleUser.getBasicProfile())

    // Useful data for your client-side scripts:
    var profile = googleUser.getBasicProfile();
    console.log("ID: " + profile.getId()); // Don't send this directly to your server!
    console.log('Full Name: ' + profile.getName());
    console.log('Given Name: ' + profile.getGivenName());
    console.log('Family Name: ' + profile.getFamilyName());
    console.log("Image URL: " + profile.getImageUrl());
    console.log("Email: " + profile.getEmail());

    // The ID token you need to pass to your backend:
    id_token = googleUser.getAuthResponse().id_token;
    showSignOutGoogleID();

    //signOut();
    if (id_token === "")
        signOut();
    else
        getActualResult_authorized(id_token);


}

function signOut() {
    var auth2 = gapi.auth2.getAuthInstance();
    auth2.signOut().then(function () {
        console.log('User signed out.');
    });

    setVisibility(true, $('#sign_out_googleID_button'));
}

function onSuccess(googleUser) {
    console.log('Logged in as: ' + googleUser.getBasicProfile().getName());
}
function onFailure(error) {
    console.log(error);
}

function renderButton() {
    gapi.signin2.render('my-signin2', {
        'scope': 'profile email',
        'width': 120,
        'height': 20,
        'longtitle': true,

        'onsuccess': onSuccess,
        'onfailure': onFailure
    });
}