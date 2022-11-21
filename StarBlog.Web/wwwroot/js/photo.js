// init Masonry
let $grid = $('#photo-grid').masonry({
    itemSelector: '.grid-item',
    percentPosition: true,
})
// layout Masonry after each image loads
$grid.imagesLoaded().progress( function() {
    $grid.masonry('layout');
});