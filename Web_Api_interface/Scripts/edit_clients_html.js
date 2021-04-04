'use strict';

var isEditingEnabled = false;
var input_images = new Map();

//here the data actually poured in the table on the page
function throwDataToThePage($resultsTable, resultFlightData, i) {

	const input_firstName = document.createElement("INPUT");
	input_firstName.setAttribute("type", "text");
	input_firstName.setAttribute("id", `firstName_${resultFlightData.iD}`);
	input_firstName.setAttribute("value", resultFlightData.FirstName);
	input_firstName.hidden = true;
	document.body.appendChild(input_firstName);

	const input_lastName = document.createElement("INPUT");
	input_lastName.setAttribute("type", "text");
	input_lastName.setAttribute("id", `lastName_${resultFlightData.iD}`);
	input_lastName.setAttribute("value", resultFlightData.LastName);
	input_lastName.hidden = true;
	document.body.appendChild(input_lastName);

	const input_address = document.createElement("INPUT");
	input_address.setAttribute("type", "text");
	input_address.setAttribute("id", `address_${resultFlightData.iD}`);
	input_address.setAttribute("value", resultFlightData.Address);
	input_address.hidden = true;
	document.body.appendChild(input_address);

	const input_phone = document.createElement("INPUT");
	input_phone.setAttribute("type", "text");
	input_phone.setAttribute("id", `phone_${resultFlightData.iD}`);
	input_phone.setAttribute("value", resultFlightData.Phone);
	input_phone.hidden = true;
	document.body.appendChild(input_phone);

	input_images[resultFlightData.iD] = resultFlightData.Image;

	fillTheTableWithData($resultsTable, {id: resultFlightData.iD, image: resultFlightData.Image, firstName: resultFlightData.FirstName, lastName: resultFlightData.LastName, address: resultFlightData.Address, phone: resultFlightData.Phone}, i);

}

function fillTheTableWithData($resultTable, valuesToFill, num) {

	$resultTable.append(`
					<tr id="_flightTable_${valuesToFill.id}" class="_flightTable">
						<td>${num + 1}</td>

						<td>
						<image id="image_${valuesToFill.id}" src="${valuesToFill.image}" width="80">
						<div class="editCustomers">
							<label class="custom-file-upload-button">
							<input type="file" onchange="addToMap($(this).prop('files')[0], ${valuesToFill.id});">
								בחר קובץ
							</label>							
						</div>
						</td>

						<td>
						<div class="notEditCustomers">${valuesToFill.firstName}</div>
						<div class="editCustomers">
							<input type="text" value="${valuesToFill.firstName}" onkeyup="$('#firstName_${valuesToFill.id}').val($(this).val());"/>
						</div>
						</td>

						<td>
						<div class="notEditCustomers">${valuesToFill.lastName}</div>
						<div class="editCustomers">
							<input type="text" value="${valuesToFill.lastName}" onkeyup="$('#lastName_${valuesToFill.id}').val($(this).val());"/>
						</div>
						</td>

						<td>
						<div class="notEditCustomers">${valuesToFill.address}</div>
						<div class="editCustomers">
							<input type="text" value="${valuesToFill.address}" onkeyup="$('#address_${valuesToFill.id}').val($(this).val());"/>
						</div>
						</td>

						<td>
						<div class="notEditCustomers">${valuesToFill.phone}</div>
						<div class="editCustomers">
							<input type="text" value="${valuesToFill.phone}" onkeyup="$('#phone_${valuesToFill.id}').val($(this).val());"/>
						</div>
						</td>
						
						<td>
						<div class="editCustomers">
							<input type="button" value="Edit" onclick="editCustomer(${valuesToFill.id}, ${num});" />
							<input type="button" value="Delete" onclick="deleteCustomer(${valuesToFill.id}, ${num});" />
						</div>
						</td>
					</tr>
					`);
}

function enableEditing() {

	if (isEditingEnabled === true) {
		setVisibility(false, $('.editCustomers'), 'block');
		setVisibility(true, $('.notEditCustomers'), 'block');
		isEditingEnabled = false;
		$('#editing_button').val('Enable editing');
	}
	else {
		setVisibility(true, $('.editCustomers'), 'block');
		setVisibility(false, $('.notEditCustomers'), 'block');
		isEditingEnabled = true;
		$('#editing_button').val('Disable editing');
    }

}

function addToMap(inputObject, key) {

	getBase64(inputObject).then(
		function (data) {
			input_images[key] = data;
			$(`#image_${key}`).attr('src', data);
		}
	)
		.catch(function (err) {
			alert('IMAGE UPLOADING TO DB FAILED!\n' + err);
        }
		);
}

//{ ID, image, firstName, lastName, address, phone }
function editCustomer(customerID, i) {
	let firstName = $(`#firstName_${customerID}`).val();
	let lastName = $(`#lastName_${customerID}`).val();
	let address = $(`#address_${customerID}`).val();
	let phone = $(`#phone_${customerID}`).val();
	let image = input_images[customerID];


	//alert(`${firstName}, ${lastName}, ${address}, ${phone}, ${image}`);

	if (authTwtToken === "" || authTwtToken === undefined)
		authTwtToken = sessionStorage.getItem(session_key);

	var ajaxPostDataConfig = {
		type: "POST",
		url: `../api/LoggedInAdministratorFacade/UpdateCustomerDetails`,
		headers: {
			'Authorization': `Bearer ${authTwtToken}`,
			'Content-Type': 'application/json',
			'From': 'local'
		},
		contentType: "application/json",
		dataType: "json",
		data: JSON.stringify({
			'Address': address,
			'FirstName': firstName,
			'LastName': lastName,
			'Image': image,
			'Phone': phone,
			'iD': customerID
		}) // request http body
	}
		$.ajax(ajaxPostDataConfig)
		.then(function (updatedCustomerData) {
			$(`#_flightTable_${customerID}`).html(`

			<td>${i + 1}</td>

						<td>
						<image id="image_${customerID}" src="${updatedCustomerData.Image}" width="80">
						<div class="editCustomers">
							<label class="custom-file-upload-button">
							<input type="file" onchange="addToMap($(this).prop('files')[0], ${customerID});">
								בחר קובץ
							</label>
						</div>
						</td>

						<td>
						<div class="notEditCustomers">${updatedCustomerData.FirstName}</div>
						<div class="editCustomers">
							<input type="text" value="${updatedCustomerData.FirstName}" onkeyup="$('#firstName_${updatedCustomerData.iD}').val($(this).val());"/>
						</div>
						</td>

						<td>
						<div class="notEditCustomers">${updatedCustomerData.LastName}</div>
						<div class="editCustomers">
							<input type="text" value="${updatedCustomerData.LastName}" onkeyup="$('#lastName_${updatedCustomerData.iD}').val($(this).val());"/>
						</div>
						</td>

						<td>
						<div class="notEditCustomers">${updatedCustomerData.Address}</div>
						<div class="editCustomers">
							<input type="text" value="${updatedCustomerData.Address}" onkeyup="$('#address_${updatedCustomerData.iD}').val($(this).val());"/>
						</div>
						</td>

						<td>
						<div class="notEditCustomers">${updatedCustomerData.Phone}</div>
						<div class="editCustomers">
							<input type="text" value="${updatedCustomerData.Phone}" onkeyup="$('#phone_${updatedCustomerData.iD}').val($(this).val());"/>
						</div>
						</td>
						
						<td>
						<div class="editCustomers">
							<input type="button" value="Edit" onclick="editCustomer(${updatedCustomerData.iD}, ${i});" />
							<input type="button" value="Delete" onclick="deleteCustomer(${updatedCustomerData.iD}, ${i});" />
						</div>
						</td>

			`);
		})
		.fail(function (err) {
			alert('FAILED TO FETCH RESULT, \nError:\n\n' + JSON.stringify(err));
			console.log(err);
		})
}

function deleteCustomer(customerID, $resultTable) {

	alert(customerID);
}


function getBase64(file) {
	return new Promise((resolve, reject) => {
		const reader = new FileReader();
		reader.readAsDataURL(file);
		reader.onload = () => resolve(reader.result);
		reader.onerror = error => reject(error);
	});
}
