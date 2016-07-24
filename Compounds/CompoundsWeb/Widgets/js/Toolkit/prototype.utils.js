//  Checks that string starts with the specific string...
if (typeof String.prototype.startsWith != 'function') {
    String.prototype.startsWith = function (str) {
        return this.slice(0, str.length) == str;
    };
}

//  Checks that string ends with the specific string...
if (typeof String.prototype.endsWith != 'function') {
    String.prototype.endsWith = function (str) {
        return this.slice(-str.length) == str;
    };
}

//  Right trim of the specific string...
if (typeof String.prototype.rtrim != 'function') {
    String.prototype.rtrim = function (s) {
        return this.replace(new RegExp(s + "*$"), '');
    };
}

//  Left trim of the specific string...
if (typeof String.prototype.ltrim != 'function') {
    String.prototype.ltrim = function (s) {
        return this.replace(new RegExp("^" + s + "*"), '');
    };
}

//  Checks that string ends with the specific string...
if (typeof String.prototype.molecularFormula != 'function') {
    String.prototype.molecularFormula = function () {
        return this.replace(/_{/g, '<sub>').replace(/}/g, '</sub>');
    };
}

//  Inserts soft hyphen every 'len' characters...
if (typeof String.prototype.softHyphen != 'function') {
    String.prototype.softHyphen = function (len) {
        var newstr = '';
        for (var index = 0; index < this.length; index = index + len) {
            if (newstr) newstr += '&shy;';
            newstr += this.slice(index, index + len);
        }

        return newstr;
    };
}

if (typeof String.prototype.random != 'function') {
    String.prototype.random = function (len) {
        if (len == null)
            len = 10;

        var text = "";
        //var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        for (var i = 0; i < len; i++)
            text += this.charAt(Math.floor(Math.random() * this.length));

        return text;
    };
}

//  Parse function string and convert it to real javascript function...
if (typeof String.prototype.parseFunction != 'function') {
    String.prototype.parseFunction = function () {
        var funcReg = /function *\(([^()]+)\)[ \n\t]*{(.*)}/gmi;
        var match = funcReg.exec(this.replace(/\n/g, ' '));

        if (match) {
            return new Function(match[1].split(','), match[2]);
        }

        return null;
    };
}

//  Capitalize first letter of each word
if (typeof String.prototype.capitalize != 'function') {
    String.prototype.capitalize = function () {
        return this.replace(/(^|\s)([a-z])/g, function (m, p1, p2) { return p1 + p2.toUpperCase(); });
    };
}

//  clear array and remove all items
if (typeof Array.prototype.clear != 'function') {
    Array.prototype.clear = function () {
        this.length = 0;
    };
}

//  split array on chunks and returns array of chunks' specified size
if (typeof Array.prototype.chunks != 'function') {
    Array.prototype.chunks = function (size) {
        var array = this;
        return [].concat.apply([],
            array.map(function (elem, i) {
                return i % size ? [] : [array.slice(i, i + size)];
            })
        );
    }
}

//  clone array and return the copy
if (typeof Array.prototype.clone != 'function') {
    Array.prototype.clone = function () {
        return this.slice(0);
    }
}

//  remove duplicates from the array
if (typeof Array.prototype.unique != 'function') {
    Array.prototype.unique = function () {
        var a = this.concat();
        for (var i = 0; i < a.length; ++i) {
            for (var j = i + 1; j < a.length; ++j) {
                if (a[i] === a[j])
                    a.splice(j--, 1);
            }
        }

        return a;
    };
}

if (typeof Date.prototype.yyyymmdd != 'function') {
    Date.prototype.yyyymmdd = function () {
        var yyyy = this.getFullYear().toString();
        var mm = (this.getMonth() + 1).toString(); // getMonth() is zero-based
        var dd = this.getDate().toString();
        return yyyy + '/' + (mm[1] ? mm : "0" + mm[0]) + '/' + (dd[1] ? dd : "0" + dd[0]); // padding
    };
}

if (typeof JSON.find != 'function') {
    JSON.find = function (obj, key, val) {
        var objects = [];
        for (var p in obj) {
            if (!obj.hasOwnProperty(p)) continue;

            if (typeof obj[p] == 'object') {
                objects = objects.concat(JSON.find(obj[p], key, val));
            }
            else if (p == key && obj[key] == val) {
                objects.push(obj);
            }
        }

        return objects;
    };
}

if (typeof JSON.bind != 'function') {
    JSON.bind = function (obj, key, val, values) {
        var objects = JSON.find(obj, key, val);

        $(objects).each(function () {
            $.extend(true, this, values);
        });
    };
}
