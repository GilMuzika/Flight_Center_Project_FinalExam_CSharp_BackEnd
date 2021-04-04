
var preloadedNnames = [];

function preloadNames(searchCriterion) {
	$.ajax({
		url: `../api/AnonimousUserFacade/PreloadNames/${searchCriterion}`
	})
		.then(function (preloadedNamesLocal) {
			preloadedNnames = preloadedNamesLocal;
			n = preloadedNamesLocal.length;
			$('#datalist').html('');
			showPreloadedNames($('#datalist').val());
		})
		.fail(function (err) {
			console.error(err);
		});
}




/*list of available options*/
var n = preloadedNnames.length; //length of datalist preloadedNnames   
function showPreloadedNames(value) {
	document.getElementById('datalist').innerHTML = '';
	//setting datalist empty at the start of function 
	//if we skip this step, same name will be repeated 

	//input query length 
	for (var i = 0; i < n; i++) {
		if (((preloadedNnames[i].toLowerCase()).indexOf(value.toLowerCase())) > -1) {
			//comparing if input string is existing in preloadedNnames[i] string

			var node = document.createElement("option");
			var val = document.createTextNode(preloadedNnames[i]);
			node.appendChild(val);

			document.getElementById("datalist").appendChild(node);
			//creating and appending new elements in data list 
		}
	}
}





var show_results_table_substitutorVisibility = false;
function getSearchResults(searchQuery, searchCriterion, sortingCriterion) {
	runNumbers(true);
	$('#show_results_table_tbody').html('');

	setVisibility(false, $("#show_results_table"));
	setVisibility(true, $("#show_results_table_substitutor"));
	$("#user_messages").html('Please wait');
	$("#user_messages").css({'border': 'none'});

	if (typeof searchQuery == 'undefined') searchQuery = '';

	var alikeQueryParameters = { searchQuery, searchCriterion, sortingCriterion };

	var ajaxPostDataConfig = {
		type: "POST", 
		url: `../api/AnonimousUserFacade/GetFlightsByQuery`,
		contentType: "application/json",
		dataType: "json",
		data: JSON.stringify(alikeQueryParameters) // request http body
	}

	$.ajax(ajaxPostDataConfig)
		.then(function (results) {
			setVisibility(true, $("#show_results_table"));
			setVisibility(false, $("#show_results_table_substitutor"));
			$resultsTable = $("#show_results_table_tbody");
			if (Array.isArray(results)) {
				$.each(results, function (i, resultFlightData) {
					console.log(resultFlightData);
					$resultsTable.append(`
					<tr id="_flightTable">
						<td>${i+1}</td>
						<td>${resultFlightData.Adorning}</td>
						<td><image src="${resultFlightData.Image}" width="40" alt="${resultFlightData.AirlineName}" title="${resultFlightData.AirlineName}"></td>
						<td>${resultFlightData.DepartureCountryName}</td>
						<td>${resultFlightData.DestinationCountryName}</td>
						<td>&nbsp;</td>
						<td>${resultFlightData.FlightDuration}</td>
						<td>${resultFlightData.EstimatedTime}</td>					
						<td>${Math.floor(Math.random() * 5) + 1}</td>
						<td>${resultFlightData.Price}</td>
					</tr>
					`);
				})
				runNumbers(false);
			}
		}
		)
		.fail(
			// what to do on error
			function (err) {
				$('#user_messages').css({ 'border': 'solid 2px red', 'width': '50%' });
				$('#user_messages').text(err.responseText);
			}
		)
}