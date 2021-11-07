function GetFeaturedProductsPerCategory(categoryIdArray) {
    function setResponse(data, status = 200, message = "OK") {
        getContext().getResponse().setBody({ status, message, data });
    }

    var collection = getContext().getCollection();
    var categoryIds = JSON.parse(categoryIdArray);
    var featuredProductsMap = {};

    categoryIds.forEach(categoryId =>
    {
        var isAccepted = collection.queryDocuments(
            collection.getSelfLink(),
            `SELECT TOP 5 * FROM c WHERE c.category.id = '${categoryId}' ORDER BY c.name`,
            function (err, feed, options) {
                if (err) throw err;
                featuredProductsMap[categoryId] = feed;
            }
        )

        if (!isAccepted) throw new Error("The query was not accepted by the server");
    })

    setResponse(featuredProductsMap);
}