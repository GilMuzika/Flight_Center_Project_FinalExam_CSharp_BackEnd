'use strict';


var authTwtToken = "";

//key for the session
const session_key = "dfshhg,hjhrgihegjsfgfdf";

async function failing(err) {
	setVisibility(false, $("#show_results_table"));
	setVisibility(true, $("#show_results_table_substitutor"));


	//alert(err.status);
	authTwtToken = "";

	//let loginFormHtml = '<table border ="1"  class="FlightsTable"><tr><td>User Name: </td><td><input type="text" id="login_iframe_username" /></td></tr> <tr><td>Password:</td><td><input type="text" id="login_iframe_password" /></td></tr></table><br /><input class="Centering" type="button" value="Login" onclick="pullAllCustomers($(\'#login_iframe_username\').val(), $(\'#login_iframe_password\').val());"/>';
	let loginFormHtml = await readRemoteTextFile('../Content/login_table.txt');

	if (err.status === 403) {
		console.log(`${err.status} error:`);
		console.log(err);
		$('#user_messages_view_clients').append('<br/>').append(err.responseText);
	}

	if (err.status === 401) {
		$('#user_messages_view_clients').append('<br/>').append('Yo\'re unauthorised, probably the username and/or the password are wrong. <br/> Please login again.');
	}

	if (err.status === 500) {
		$('#user_messages_view_clients').css({ 'border': 'solid 2px red', 'width': '50%' });
		$('#user_messages_view_clients').html('Something wrong happened, probably your token is corrupt, please sign in again');
		$('#user_messages_view_clients').append('<br/>').append(loginFormHtml);
	}

	if (err.status === 417) {
		$('#user_messages_view_clients').css({ 'width': '50%' });
		//$('#user_messages_view_clients').html('<iframe id="myIframe" src="../Content/login_iframe.html" title="login iframe page"></iframe>');
		$('#user_messages_view_clients').html(loginFormHtml);
	}

}


function pullAllCustomers(username, password) {
	swal('שלום');
	showSignOutGoogleID();

	if (sessionStorage.getItem(session_key) === null || sessionStorage.getItem(session_key) === "" || sessionStorage.getItem(session_key) === undefined) {

		//if (username === "" || username === undefined) username = "username";
		//if (password === "" || password === undefined) password = "password";

		var ajaxPostDataConfig = {
			type: "POST",
			url: `../api/jwt/createJwtToken`,
			contentType: "application/json",
			dataType: "json",
			data: JSON.stringify({
				"username": username,
				"password": password
			}) // request http body
		}

		$.ajax(ajaxPostDataConfig)
			.then(function (token) {

				//here the JWT tken is got into the page
				authTwtToken = token.Token;

				//I'm also putting the token in a session
				sessionStorage.setItem(session_key, token.Token);

				getActualResult_authorized(authTwtToken);
			})

			// the "fail" section is get the error in the case if login failed, display the error message, and, in th case of attempt of login without actually login
			// (login from a page without a  form, both the username and the password are empty strings, status code 417, display the iframe element with the login form)
			.fail(
				// what to do on error
				function (err) {
					failing(err);
				}
			)
	}
	else {
		if(authTwtToken === "") {
			authTwtToken = sessionStorage.getItem(session_key);
        }
		getActualResult_authorized(authTwtToken);
	}
}



function getActualResult_authorized(authToken) {
	setVisibility(false, $("#show_results_table"));
	setVisibility(true, $("#show_results_table_substitutor"));
	$("#user_messages_view_clients").html('Please wait');
	$("#user_messages_view_clients").css({ 'border': 'none' });

	$.ajax({
		url: `../api/LoggedInCustomerFacade/GetAllCustomers`,
		headers: {
			'Authorization': `Bearer ${authToken}`,
			'Content-Type': 'application/json',
			'From': 'local'
		}
	})
		.then(function (results) {
			$('#user_messages_view_clients').html('');
			setVisibility(false, $("#show_results_table_substitutor"));
			setVisibility(true, $("#show_results_table"));
			const $resultsTable = $("#show_results_table_tbody");

			if (Array.isArray(results)) {
				$.each(results, function (i, resultFlightData) {
					//console.log(resultFlightData);
					//by this function the data actually poured in the table on the page
					throwDataToThePage($resultsTable, resultFlightData, i);
					setVisibility(true, $('#editing_button'));
				})
				runNumbers(false);
			}
		}
		)
		.fail(
			// what to do on error
			function (err) {
				failing(err);
				console.log(err);
			}
		)
}



function runNumbers(trigger) {
	 let i = 1;
	var runningNumbersInterval;
	if (trigger == true) {
		runningNumbersInterval = setInterval(function () {
			$('#running_numbers').text(i++);
		}, 100);
	}
	if (trigger == false) {
		clearInterval(runningNumbersInterval);
	}
}

function showSignOutGoogleID() {
	if (id_token === undefined || id_token === "")
		setVisibility(false, $('#sign_out_googleID_button'))
	else setVisibility(true, $('#sign_out_googleID_button'))
}

//enables or disables the visibility of the DOM element that passed as the second parameter,
//depending on the first boolean parameter
function setVisibility(triggerBool, $dom_element, displayMode='table') {
	let visibility = null;
	let display = null;
	triggerBool == true ? visibility = 'visible' : visibility = 'hidden';
	triggerBool == true ? display = displayMode : display = 'none';

	//$dom_element.css({ visibility: visibility, display: display });
	$dom_element.css({ visibility, display });
	return triggerBool;
}


async function readRemoteTextFile(pathToFile) {
	var dataGlogInFunc = 'If you see this, there was no communication with the server at all, the promice was never return a value and the function returned this very default value.';

	await $.get(pathToFile, function (data) {
		//console.log(data);
		dataGlogInFunc = data;
	}, 'text')
		.fail(function (err) {
			console.log(err.responseText);
			dataGlogInFunc = `If you see this text, the server returned error ${err.status}, error message: \n\n ${err.responseText}`;
		});
	return dataGlogInFunc;
}

