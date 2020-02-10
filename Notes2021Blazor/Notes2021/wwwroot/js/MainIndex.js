/*--------------------------------------------------------------------------
**
**  Copyright (c) 2019, Dale Sinder
**
**  Name: MainIndex.js
**
**  Description:
**      Java Script for Notes 2019 Main Index Page
**
**  This program is free software: you can redistribute it and/or modify
**  it under the terms of the GNU General Public License version 3 as
**  published by the Free Software Foundation.
**  
**  This program is distributed in the hope that it will be useful,
**  but WITHOUT ANY WARRANTY; without even the implied warranty of
**  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
**  GNU General Public License version 3 for more details.
**  
**  You should have received a copy of the GNU General Public License
**  version 3 along with this program in file "license-gpl-3.0.txt".
**  If not, see <http://www.gnu.org/licenses/gpl-3.0.txt>.
**
**--------------------------------------------------------------------------
*/

function code(e) {
    e = e || window.event;
    return e.keyCode || e.which;
}
window.onload = function () {

    document.onkeypress = function (e) {
        var y = document.getElementById("FileName");
        if ((y.value.length) > 1) {
            return;
        }

        var key = code(e);
            // do something with key
            var x;
            if (key === 49) {
                x = document.getElementById("nf1");
                if (x !== null) {
                    x.click();
                }
            } else if (key === 50) {
                x = document.getElementById("nf2");
                if (x !== null) {
                    x.click();
                }
            } else if (key === 51) {
                x = document.getElementById("nf3");
                if (x !== null) {
                    x.click();
                }
            } else if (key === 52) {
                x = document.getElementById("nf4");
                if (x !== null) {
                    x.click();
                }
            } else if (key === 53) {
                x = document.getElementById("nf5");
                if (x !== null) {
                    x.click();
                }
            } else if (key === 54) {
                x = document.getElementById("nf6");
                if (x !== null) {
                    x.click();
                }
            } else if (key === 55) {
                x = document.getElementById("nf7");
                if (x !== null) {
                    x.click();
                }
            } else if (key === 56) {
                x = document.getElementById("nf8");
                if (x !== null) {
                    x.click();
                }
            }

            //else {
            //    alert(key);
            //}
    };
};