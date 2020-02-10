// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
/*--------------------------------------------------------------------------
**
**  Copyright (c) 2019, Dale Sinder
**
**  Name: site.js
**
**  Description:
**      Java Script for Notes 2020 application pages
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

//Write your Javascript code.

//// Enable pusher logging - don't include this in production
//Pusher.log = function (message) {
//    if (window.console && window.console.log) {
//        window.console.log(message);
//    }
//};
//Pusher.logToConsole = true;

// Dev Key 5941bc1e7cf86cd694fa
// Staging Key 824d479e7db8e7f6740e
// Production Key fec17aef3b0f3709af41

var username = document.getElementById("notesusername");

var pathbase = document.getElementById("pathbase").innerHTML;

var pusherkey = document.getElementById("pusherkey");
var pushercluster = document.getElementById("pushercluster");

var membercount = 0;

setInterval(function () {
    var clock = document.getElementById('digitalclock');
    if (clock !== null) {
        var time = new Date().toTimeString();
        //var split = time.split('(');
        clock.innerHTML = time;  /*split[0];*/
    }
}, 1000);

var pusher = new Pusher(pusherkey.innerHTML, {
    authEndpoint:  pathbase + '/Home/PusherAuth',
    encrypted: true,
    cluster: pushercluster.innerHTML,
    forceTLS: true
});

var channel = pusher.subscribe('notes-channel');

var presence = pusher.subscribe('presence-channel');

var privatechannel = pusher.subscribe('notes-data-' + username.innerHTML);

channel.bind('sys_message_event', function (data) {
    document.getElementById('sysmessage').innerHTML = data.message;
    $('#mySysMessage').modal('show');

    //alert(data.message);
});

channel.bind('import_status_message_event', function (data) {
    var status_message = data.newmessage;
    document.getElementById('import_status').innerHTML = status_message;
});

presence.bind('pusher:subscription_succeeded', function (members) {
    members.each(function (member) {
        window.add_member(member.id, member.info);
    });
});
presence.bind('pusher:member_added', function (member) {
    window.add_member(member.id, member.info);
});

presence.bind('pusher:member_removed', function (member) {
    window.remove_member(member.id, member.info);
});

presence.bind('chat_request_event', function (data) {
    if (username === null)
        return;
    if (username.innerHTML !== data.username)
        return;

    document.getElementById('oktochat').innerHTML = data.username;
    document.getElementById("sysmessagechat").innerHTML = data.message;
    $('#myChatMessage').modal('show');

    //var r = confirm(data.message);
    //if (r === true) {
    //    window.open('/Home/Chat/' + data.username + '?id2=false');
    //}
});

function oktochatfunc() {
    $('#myChatMessage').modal('hide');
    window.open(pathbase + '/Home/Chat/' + document.getElementById('oktochat').innerHTML + '?id2=false');
}

privatechannel.bind('update-time', function (data) {
    var homepagetime = document.getElementById('homepagetime');
    if (homepagetime === null)
        return;
// ReSharper disable once QualifiedExpressionMaybeNull
    homepagetime.innerHTML = data.message;
});

// chatChannel is target user specific
var chatChannel = pusher.subscribe('private-notes-chat-' + $('#displayname2').val());

chatChannel.bind('show_chat_message_event', function (data) {
    $('#discussion').append('<li><strong>' + htmlEncode(data.username)
        + '</strong>: ' + htmlEncode(data.message) + '</li>');
});

chatChannel.bind('client-append-message', function (data) {
    var d = new Date();
    $('#discussion').append('<li><strong>' + htmlEncode(data.username)
        + '</strong> @ ' + d.toLocaleTimeString() + ' : <strong>' + htmlEncode(data.message) + '</strong></li>');
});

function sendChatMessage() {
    chatChannel.trigger('client-append-message', { username: $('#displayname').val(), message: $('#message').val() });
    var d = new Date();
    $('#discussion').append('<li><strong>' + htmlEncode($('#displayname').val())
        + '</strong> @ ' + d.toLocaleTimeString() + ' : <strong>' + htmlEncode($('#message').val()) + '</strong></li>');
    // Clear text box and reset focus for next comment.
    $('#message').val('').focus();
}

// send button on click handler
$('#chatsendmessage').click(function () {
    sendChatMessage();
});

$('#message').keypress(function (event) {
    var keycode = (event.keyCode ? event.keyCode : event.which);
    if (keycode === '13') {
        sendChatMessage();
    }
});


// This optional function html-encodes messages for display in the page.
function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}
