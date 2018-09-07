"use strict";
var SEARCH_TOKEN_MIN_LENGTH = 3;
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
            copyText(successResult, element);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            var toolTip = $("<div class='ob-copyTooltip'>Copied!</div>");
            toolTip.insertAfter(copyIconElement);

            toolTip.text(textStatus);
            toolTip.css("background-color", "#f00");
            toolTip.css("color", "#fff");
            toolTip.show();

            setTimeout(function () {
                toolTip.remove();
            }, 1000);
        }
    });
}

/**
 * Performs a copy to clipboard for the selected login
 * The click logic is complex:
 * @param {any} element
 */
function loginCopy(element) {
    var assetId = element.data("id");
    var textAfter = $(element).get(0).nextSibling.textContent.trim();
    copyText(textAfter, element);
}

/**
 * Performs a copy to clipboard for the given text and shows a tooltip message for success/error near the given parentElement
 * @param {string} text The text to copy to clipboard
 * @param {any} parentElement The element to put the tooltip message next to
 */
function copyText(text, parentElement) {
    let copyFrom = document.createElement("textarea");
    copyFrom.style.width = 0;
    copyFrom.style.height = 0;
    copyFrom.style.position = 'fixed';
    copyFrom.style.top = '-1000px';
    copyFrom.style.left = '-1000px';
    document.body.appendChild(copyFrom);

    // This element here is added only for IE check - if the copy-paste is successed - then the user allowed the browser to copy
    let pasteTo = document.createElement("textarea");
    pasteTo.style.width = 0;
    pasteTo.style.height = 0;
    pasteTo.style.position = 'fixed';
    pasteTo.style.top = '-1000px';
    pasteTo.style.left = '-1000px';
    document.body.appendChild(pasteTo);

    copyFrom.textContent = text;
    copyFrom.select();
    var canCopy = document.execCommand("copy");
    pasteTo.select();
    var canPaste = document.execCommand('paste');

    var copySuccess;
    if (canPaste) {
        // This is the actual check (for IE only) - if the copy-paste is successed - then the user allowed the browser to copy
        copySuccess = pasteTo.value === copyFrom.value;

        // Since IE supports paste,
        // if user allowed clipboard: copySuccess === true
        // else: copySuccess === false
        if (copySuccess) {
            var toolTip = $("<div class='ob-copyTooltip'>Copied!</div>");
            toolTip.insertAfter(parentElement);

            setTimeout(function () {
                toolTip.remove();
            }, 2000);
        }
        else {
            var toolTip = $("<div class='ob-copyTooltip'>NOT COPIED! Please, refresh the page and allow your browser to copy!</div>");
            toolTip.insertAfter(parentElement);

            toolTip.css("background-color", "#f00");
            toolTip.css("color", "#fff");
            toolTip.css("width", "300px");
            toolTip.css("left", "80px");

            setTimeout(function () {
                toolTip.remove();
            }, 2000);
        }
        
    }
    else if (canPaste == false) { // This means that the Browser is not IE (Chrome, Firefox, Edge, etc.)
        var toolTip = $("<div class='ob-copyTooltip'>Copied!</div>");
        toolTip.insertAfter(parentElement);

        setTimeout(function () {
            toolTip.remove();
        }, 2000);
    }
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

        if (iconType == "password") {
            passwordCopy($(this));
        }
        else if (iconType == "login") {
            loginCopy($(this));
        }
    });
};

function bindSearchAssetInput() {
    $("div.dropdown#assetSearchAutocomplete input").on('focusin', function (e) {
        if ($(this).val().length >= SEARCH_TOKEN_MIN_LENGTH) {
            $(this).triggerHandler('input');
        }
    });

    $(document).on('click', function (e) {
        var dropdown = $('div.dropdown#assetSearchAutocomplete ul.dropdown-menu');
        if (!dropdown.find($(e.target)).length) {
            dropdown.addClass('invisible-asset-holder');
        }
    });

    var timeout = null;
    $("div.dropdown#assetSearchAutocomplete input").on('input', function (e) {
        var searchPrefix = $(this).val(),
            getUrl = "/Home/GetAssetResults?searchToken=" + searchPrefix,
            dropdown = $(this).siblings('ul.dropdown-menu'),
            spinner = $(this).parents('div.row').find('div.loader');

        if (searchPrefix.length < SEARCH_TOKEN_MIN_LENGTH) {
            spinner.hide();
            dropdown.addClass('invisible-asset-holder');
            return;
        }

        spinner.show();
        clearTimeout(timeout);
        timeout = setTimeout(function () {
            var context = {
                input: this,
                searchPrefix: searchPrefix,
                spinner: spinner,
                dropdown: dropdown
            };

            $.ajax({
                type: 'GET',
                url: getUrl,
                contentType: 'application/json',
                cache: false,
                success: function (successResult) {
                    if ($(this.input).val() !== this.searchPrefix) {
                        return;
                    }

                    var dropdown = this.dropdown;
                    if (successResult.length) {
                        successResult.sort(function (a, b) {
                            return a.displayTitle.toString().toLowerCase() > b.displayTitle.toString().toLowerCase();
                        });

                        dropdown.empty();
                        for (var i = 0; i < successResult.length; i++) {
                            var entry = successResult[i];
                            dropdown.append('<li class="asset"><a href="/Projects/Details/' + entry.projectId + '">' + entry.displayTitle + '</a></li>');
                        }

                        dropdown.removeClass('invisible-asset-holder');
                    }
                    else {
                        dropdown.addClass('invisible-asset-holder');
                    }

                    this.spinner.hide();
                }.bind(context),
                error: function () {
                    spinner.hide();
                    dropdown.addClass('invisible-asset-holder');
                }.bind(context)
            });
        }.bind(this), 800);
    });
};

$(document).ready(function () {
    // Bind all password label click functions
    bindAllPasswordLabels();
    bindAllPasswordCopyToClipboard();
    // bind asset search box
    bindSearchAssetInput();
});