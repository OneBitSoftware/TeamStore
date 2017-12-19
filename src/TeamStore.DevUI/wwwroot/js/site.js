"use strict";

/**
 * Performs a click event for a password mask
 * Appends the anti-forgery token in the post message and header.
 * The click logic is complex:
 * 1. The mask click event is bound.
 * 2. on click it is replaced with loading and the ajax request is released. The click handler is removed.
 * 3. the ajax response replaces the mask body with the result, both fail and success. A timer is kicked off.
 * 4. the timer returns the mask and rebinds the click event to the clicked span. It appears to be recursive.
 * @param {any} element
 */
function passwordClick(element) {
    var assetId = element.data("id");
    var postUrl = "/Projects/GetPassword";
    var token = $('input[name="__RequestVerificationToken"]').val(); // prepare token
    var projectId = $("input#projectId").val();
    var postData = {
        __RequestVerificationToken: token,
        formDigest: '2134',
        projectId: projectId,
        assetId: assetId
    };

    // 2. replace with loading, unbind click event and release ajax request with token
    var containerSpan = element;
    containerSpan.html("Loading...");
    containerSpan.removeClass("ob-pointer");
    containerSpan.off("click");

    $.ajax({
        type: 'POST',
        url: postUrl,
        data: JSON.stringify(postData),
        contentType: 'application/json',
        cache: false,
        headers: { 'RequestVerificationToken': token },
        success: function (successResult) {
            // 3. replace mask with result and start timer
            containerSpan.html(successResult);

            setTimeout(function () {
                // 4. return mask and rebind click event
                containerSpan.html("******");
                containerSpan.addClass("ob-pointer");
                containerSpan.on('click', function (e) {
                    passwordClick($(this));
                })
            }, 2000);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            // 3. replace mask with result and start timer
            containerSpan.html(textStatus);
            setTimeout(function () {
                // 4. return mask and rebind click event
                containerSpan.html("******");
                containerSpan.addClass("ob-pointer");
                containerSpan.on('click', function (e) {
                    passwordClick($(this));
                })
            }, 2000);
        }
    });
}

/**
 * Performs a copy to clipboard for the selected password
 * Appends the anti-forgery token in the post message and header.
 * The click logic is complex:
 * @param {any} element
 */
function passwordCopy(element) {
    var assetId = element.data("id");
    var postUrl = "/Projects/GetPassword";
    var token = $('input[name="__RequestVerificationToken"]').val(); // prepare token
    var projectId = $("input#projectId").val();
    var postData = {
        __RequestVerificationToken: token,
        formDigest: '2134',
        projectId: projectId,
        assetId: assetId
    };

    $.ajax({
        type: 'POST',
        url: postUrl,
        data: JSON.stringify(postData),
        contentType: 'application/json',
        cache: false,
        async: false, //The AJAX should be SYNCHRONOUS, in order document.execCommand("copy") to work successfully
        headers: { 'RequestVerificationToken': token },
        success: function (successResult) {
            let copyFrom = document.createElement("textarea");
            document.body.appendChild(copyFrom);
            copyFrom.textContent = successResult;
            copyFrom.select();
            document.execCommand("copy");
            $(copyFrom).remove();

            var toolTip = $("div.ob-copyTooltip[data-id=" + assetId + "]");
            toolTip.show();

            setTimeout(function () {
                toolTip.hide();
            }, 1000);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            var toolTip = $("div.ob-copyTooltip[data-id=" + assetId + "]");
            toolTip.text(textStatus);
            toolTip.css("background-color", "#f00");
            toolTip.css("color", "#fff");
            toolTip.show();

            setTimeout(function () {
                toolTip.hide();
            }, 1000);
        }
    });
}

/**
 * Binds the click event on all password labels
 */
function bindAllPasswordLabels() {
    $("span.ob-passwordLabel").on('click', function (e) {
        window.appInsights.trackEvent("passwordClicked", $(this).text());
        passwordClick($(this));
    });
};

/**
 * Binds the click event on all password copy-to-clipboard
 */
function bindAllPasswordCopyToClipboard() {
    $("button.ob-copyIcon").on('click', function (e) {
        var assetId = $(this).data("id");
        var iconType = $(this).data("type");
        //console.log(assetId);
        //console.log(iconType);

        passwordCopy($(this));
    });
};

$(document).ready(function () {
    // Bind all password label click functions
    bindAllPasswordLabels();
    bindAllPasswordCopyToClipboard();
});