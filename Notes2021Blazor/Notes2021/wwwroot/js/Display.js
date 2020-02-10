﻿/*--------------------------------------------------------------------------
**
**  Copyright (c) 2019, Dale Sinder
**
**  Name: Display.js
**
**  Description:
**      Java Script for Notes 2020 display pages
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
    return (e.keyCode || e.which);
}
window.onload = function () {
    document.onkeypress = function(e) {
        var key = code(e);
        // do something with key
        var x;
        if (key === 73) {
            x = document.getElementById("toIndex");
            if (x !== null) {
                x.click();
            }
        } else if (key === 32 && e.shiftKey !== true) {
            x = document.getElementById("toSequence");
            if (x !== null) {
                x.click();
            }
        } else if (key === 32 && e.shiftKey === true) {
            x = document.getElementById("continueSearch");
            if (x !== null) {
                x.click();
            }
        } else if (key === 13 && e.shiftKey === true) {
            x = document.getElementById("toNextBase");
            if (x !== null) {
                x.click();
            }
        } else if (key === 47) {
            x = document.getElementById("toNextNote");
            if (x !== null) {
                x.click();
            }
        } else if (key === 119 && e.shiftKey === true) {
            x = document.getElementById("toPrevBase");
            if (x !== null) {
                x.click();
            }
        } else if (key === 119) {
            x = document.getElementById("toPrevNote");
            if (x !== null) {
                x.click();
            }
        } else if (key === 70 && e.shiftKey === true) {
            x = document.getElementById("toForward");
            if (x !== null) {
                x.click();
            }
        } else if (key === 77 && e.shiftKey === true) {
            x = document.getElementById("toMark");
            if (x !== null) {
                x.click();
            }
        } else if (key === 109 && e.shiftKey === false) {
            x = document.getElementById("toMail");
            if (x !== null) {
                x.click();
            }
        } else if (key === 78 && e.shiftKey === true) {
            x = document.getElementById("toNewResponse");
            if (x !== null) {
                x.click();
            }
        } else if (key === 69 && e.shiftKey === true) {
            x = document.getElementById("toEdit");
            if (x !== null) {
                x.click();
            }
        } else if (key === 68 && e.shiftKey === true) {
            x = document.getElementById("toDelete");
            if (x !== null) {
                x.click();
            }
        } else if (key === 67 && e.shiftKey === true) {
            x = document.getElementById("toCopy");
            if (x !== null) {
                x.click();
            }
        } else if (key === 72 && e.shiftKey === true) {
            x = document.getElementById("asHtml");
            if (x !== null) {
                x.click();
            }
        } else if (key === 104 && e.shiftKey === false) {
            x = document.getElementById("asHtmlAlt");
            if (x !== null) {
                x.click();
            }
        } else if (key === 83 && e.shiftKey === true) {
            x = document.getElementById("toSearch");
            if (x !== null) {
                x.click();
            }
        } else if (key === 90) {
            $('#myModal').modal('show');
        } else if (key === 112) {
            $('#myModal').modal('show');
        }
    };
};