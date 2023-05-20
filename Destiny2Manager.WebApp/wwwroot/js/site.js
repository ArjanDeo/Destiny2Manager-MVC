$.ajax({
    type: "POST",
    url: "/Destiny/EquipAurvandilAsync",
    success: function () {
        console.log("Success");
    },
    error: function (xhr, status, error) {
        console.log(xhr.responseText);
    }
});