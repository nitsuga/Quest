
function setStore(cname, cvalue, exdays) {
    localStorage.setItem(cname, cvalue);
}

function getStore(cname) {
    return localStorage.getItem(cname);
}

function getStoreAsBool(cname) {
    return (localStorage.getItem(cname) === "true") ? true : false;
}

// set a bootstrap slider from the store
function setSliderFromStore(name) {
    if (name === '#')
        return;
    var v = getStoreAsBool(name);
    setSlider(name, v);
}

// save bootstrap slider value
function setStoreFromSlider(name) {
    if (name === '#')
        return;
    //var v = !($(name).hasClass("off"));
    var v = $(name).prop('checked');
    setStore(name, v, 365);
}


function setSlider(name, position) {
    $(name).bootstrapToggle(position ? 'on':'off');
    ////alert(position);
    //var c = $(name).find("input[type=checkbox]");
    //if (position == false) {
    //    var offtoggle = $(name).attr('class', 'toggle ' + $(name).find('.toggle-off').attr('class').replace(/toggle-off/g, ''));
    //    c.prop("checked", false);
    //    $(name).addClass('off');
    //}
    //else {
    //    $(name).attr('class', 'toggle ' + $(name).find('.toggle-on').attr('class').replace(/toggle-on/g, ''));
    //    c.prop("checked", true);
    //    $(name).removeClass('off');
    //}
}

function toggleSlider(name) {
    var v = !($(name).hasClass("off"));
    v = !v;
    setStore(name, v, 365);
    setSlider(name, v);
}

// support the case where localstorage is not intrinsically available.
if (!window.localStorage) {
    Object.defineProperty(window, "localStorage", new (function () {
        var aKeys = [], oStorage = {};
        Object.defineProperty(oStorage, "getItem", {
            value: function (sKey) { return sKey ? this[sKey] : null; },
            writable: false,
            configurable: false,
            enumerable: false
        });
        Object.defineProperty(oStorage, "key", {
            value: function (nKeyId) { return aKeys[nKeyId]; },
            writable: false,
            configurable: false,
            enumerable: false
        });
        Object.defineProperty(oStorage, "setItem", {
            value: function (sKey, sValue) {
                if (!sKey) { return; }
                document.cookie = escape(sKey) + "=" + escape(sValue) + "; expires=Tue, 19 Jan 2038 03:14:07 GMT; path=/";
            },
            writable: false,
            configurable: false,
            enumerable: false
        });
        Object.defineProperty(oStorage, "length", {
            get: function () { return aKeys.length; },
            configurable: false,
            enumerable: false
        });
        Object.defineProperty(oStorage, "removeItem", {
            value: function (sKey) {
                if (!sKey) { return; }
                document.cookie = escape(sKey) + "=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/";
            },
            writable: false,
            configurable: false,
            enumerable: false
        });
        this.get = function () {
            var iThisIndx;
            for (var sKey in oStorage) {
                iThisIndx = aKeys.indexOf(sKey);
                if (iThisIndx === -1) { oStorage.setItem(sKey, oStorage[sKey]); }
                else { aKeys.splice(iThisIndx, 1); }
                delete oStorage[sKey];
            }
            for (aKeys; aKeys.length > 0; aKeys.splice(0, 1)) { oStorage.removeItem(aKeys[0]); }
            for (var aCouple, iKey, nIdx = 0, aCouples = document.cookie.split(/\s*;\s*/) ; nIdx < aCouples.length; nIdx++) {
                aCouple = aCouples[nIdx].split(/\s*=\s*/);
                if (aCouple.length > 1) {
                    oStorage[iKey = unescape(aCouple[0])] = unescape(aCouple[1]);
                    aKeys.push(iKey);
                }
            }
            return oStorage;
        };
        this.configurable = false;
        this.enumerable = true;
    })());
}