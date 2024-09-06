var printBtn = document.getElementById("print-receipt-btn");
if (printBtn)
    printBtn.addEventListener("click", function () { window.print(); }, false);