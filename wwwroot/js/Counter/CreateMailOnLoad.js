var postalCodes = $('#PostalCodeId'); // cache the element
var choosenPaths = $('#ChoosenPath');

$('#EndLocationId').change(function () {
    debugger;
    $.getJSON("/Counter/Mail/RelatedPostalCodes", { locationId: $(this).val() }, function (response) {
        // clear and add default (null) option
        debugger;
        postalCodes.empty();
        $.each(response, function (index, item) {
            postalCodes.append($('<option></option>').val(item.value).text(item.text));
        });
    });
    $.getJSON("/Counter/Mail/DeliveryPath", { source: $("#StartLocationId").val(), target: $("#EndLocationId").val() }, function (response) {
        // clear and add default (null) option
        debugger;
        choosenPaths.empty();
        $.each(response, function (index, item) {
            choosenPaths.append($('<option></option>').val(item.value).text(item.text));
        });
    });
});
