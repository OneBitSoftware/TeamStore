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
        console.log(assetId);
        console.log(iconType);
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
    
    $("div.dropdown#assetSearchAutocomplete input").on('input', function (e) {
        var searchToken = $(this).val(),
            getUrl = "/Home/GetAssetResults?searchToken=" + searchToken,
            dropdown = $(this).siblings('ul.dropdown-menu'),
            spinner = $(this).parents('div.row').find('div.loader');

        if (searchToken.length < SEARCH_TOKEN_MIN_LENGTH) {
            spinner.hide();
            dropdown.addClass('invisible-asset-holder');
            return;
        }

        spinner.show();
        var context = {
            input: this,
            token: searchToken,
            spinner: spinner,
            dropdown: dropdown
        };

        $.ajax({
            type: 'GET',
            url: getUrl,
            contentType: 'application/json',
            cache: false,
            success: function (successResult) {
                if ($(this.input).val() !== this.token) {
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
    });
};

$(document).ready(function () {
    // Bind all password label click functions
    bindAllPasswordLabels();
    bindAllPasswordCopyToClipboard();
    // bind asset search box
    bindSearchAssetInput();
});