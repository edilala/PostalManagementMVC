﻿// globals
var mailAdminChartByLocation = null;

function GetAdminChartDataByLocation() {
    return new Promise(function (resolve, reject) {
        debugger;
        var fromDate = document.getElementById("FromDate");
        var toDate = document.getElementById("ToDate");
        var fromDateVal = null;
        var toDateVal = null;
        var reqBody = {
            FromDate: fromDate && fromDate.value,
            ToDate: toDate && toDate.value,
        }
        $.ajax({
            type: "POST",
            url: '/Admin/Mail/GetChartDataByLocation',
            data: JSON.stringify(reqBody),
            contentType: "application/json; charset=utf-8",
            dataType: "json"
        }).done(function success(result) {
            debugger;
            return resolve({ success: true, data: result });
            ShowHideLoading(false);

        }).fail(function errorFunc(error) {
            debugger;
            var mess = "";
            if (error && error.responseText && error.responseText != "")
                mess = error.responseText;

            return reject({ success: false, message: mess });
            ShowHideLoading(false);
        });
    });
}

function OnLoadAdminByLocation() {
    GetAdminChartDataByLocation().then(function success(result) {
        debugger;
        if (result && result.success && result.data)
            BuildChartByLocation(result.data);
    }).catch(function errorFunc(error) {
        debugger;

    });
}

function BuildChartByLocation(barItems) {

    const ctx = document.getElementById('counter-barchart');
    debugger;
    var labelsArr = [];
    var columnValArr = [];

    for (var i = 0; i < barItems.length; i++) {
        labelsArr.push(barItems[i].key);
        columnValArr.push(barItems[i].value);
    }

    if (mailAdminChartByLocation)
        mailAdminChartByLocation.destroy();


    mailAdminChartByLocation = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labelsArr,
            datasets: [{
                label: 'Mails processed by location',
                data: columnValArr,
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}

// starting scripts

var refreshBtn = document.getElementById("refresh-chart-btn");
if (refreshBtn)
    refreshBtn.addEventListener("click", OnLoadAdminByLocation, false);

OnLoadAdminByLocation();