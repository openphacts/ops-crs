var client = new ZeroClipboard( document.getElementById("copy-button"), {
  moviePath: "ZeroClipboard.swf"
} );

client.on( "load", function(client) {
  alert( "movie is loaded" );

  client.on( "complete", function(client, args) {
    // `this` is the element that was clicked
    this.style.display = "none";
    alert("Copied text to clipboard: " + args.text );
  } );
} );
