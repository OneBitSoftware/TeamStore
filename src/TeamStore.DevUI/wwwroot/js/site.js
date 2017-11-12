"use strict";

function hidePassword() {

};

function bindPasswordLabels() {
    $("span.ob-passwordLabel").on("click", function () {
        var assetId = $(this).data("id");
        var postUrl = "/Projects/GetPassword";
        var token = $('input[name="__RequestVerificationToken"]').val();
        var projectId = $("input#projectId").val();
        var postData = {
            __RequestVerificationToken: token,
            formDigest: '2134',
            projectId: projectId,
            assetId: assetId
        };

        var containerSpan = $(this);
        containerSpan.html("Loading...");
        containerSpan.removeClass("ob-pointer");

        $.ajax({
            type: 'POST',
            url: postUrl,
            data: JSON.stringify(postData),
            dataType: 'json',
            contentType: 'application/json',
            cache: false,
            headers: { 'RequestVerificationToken': token },
            success: function (successResult) {
                containerSpan.html(successResult);
                containerSpan.off("click");

                setTimeout(function () {
                    containerSpan.html("******");
                    containerSpan.addClass("ob-pointer");
                    bindPasswordLabels();
                }, 2000);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                $(this).html(textStatus);
            }
        });
    });
}

$(document).ready(function () {
    bindPasswordLabels();
});