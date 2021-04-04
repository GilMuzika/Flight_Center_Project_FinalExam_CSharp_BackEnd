'use strict';

//here the data actually poured in the table on the page
function throwDataToThePage($resultsTable, resultFlightData, i) {
	$resultsTable.append(`
					<tr id="_flightTable">
						<td>${i + 1}</td>
						<td><image src="${resultFlightData.Image}" width="80"></td>
						<td>${resultFlightData.FirstName}</td>
						<td>${resultFlightData.LastName}</td>
						<td>${resultFlightData.Address}</td>
						<td>${resultFlightData.Phone}</td>
					</tr>
					`);
}