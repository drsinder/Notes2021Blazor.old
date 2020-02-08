function inittinymce(s) {
    tinymce.init({
        selector: "textarea#mynote",
        theme: "modern",
        browser_spellcheck: true,

        width: 950,
        height: 300,
        max_width: 950,
        max_height: 500,
        resize: 'both',
        plugins: [
            "advlist autolink link image lists charmap print preview hr anchor pagebreak spellchecker fullscreen",
            "searchreplace wordcount visualblocks visualchars code codesample insertdatetime media nonbreaking",
            "save table contextmenu directionality emoticons paste textcolor tabfocus"
        ],
        relative_urls: false,
        convert_urls: false,
        contextmenu: "link image inserttable | cell row column deletetable",
        toolbar: "insertfile undo redo | styleselect fontselect fontsizeselect | bold italic underline | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image media codesample | print preview | nonbreaking forecolor backcolor emoticons insertdatetime fullscreen",
        codesample_languages: [
            { text: 'C#', value: 'csharp' },
            { text: 'C', value: 'c' },
            { text: 'C++', value: 'cpp' },
            { text: 'HTML/XML', value: 'markup' },
            { text: 'JavaScript', value: 'javascript' },
            { text: 'CSS', value: 'css' },
            { text: 'Python', value: 'python' },
            { text: 'Java', value: 'java' }
        ],
        style_formats: [
            { title: 'Bold text', inline: 'b' },
            { title: 'Red text', inline: 'span', styles: { color: '#ff0000' } },
            { title: 'Red header', block: 'h1', styles: { color: '#ff0000' } },
            { title: 'Example 1', inline: 'span', classes: 'example1' },
            { title: 'Example 2', inline: 'span', classes: 'example2' },
            { title: 'Table styles' },
            { title: 'Table row 1', selector: 'tr', classes: 'tablerow1' }
        ]
    });
    //window.alert("TinyMCE Init");
}

function gettinymcecontent(s) {
    stuff = tinymce.activeEditor.getContent();
    return stuff;
}

function tinyfocus(s) {
    tinyMCE.activeEditor.focus();
}

function setfocus(elm) {
    var arrow = document.getElementById(elm);
    arrow.focus();
}

function setselect0(s) {
    document.getElementById(s).selectedIndex = '0';
}

function setlocation(elm) {
    var arrow = document.getElementById(elm);
    arrow.scrollIntoView(true);
}

function saveAsFile(filename, bytesBase64) {
    var link = document.createElement('a');
    link.download = filename;
    link.href = "data:application/octet-stream;base64," + bytesBase64;
    document.body.appendChild(link); // Needed for Firefox
    link.click();
    document.body.removeChild(link);
}