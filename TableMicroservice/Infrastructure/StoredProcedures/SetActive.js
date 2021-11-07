function SetActive(tableId, active) {
    var collection = getContext().getCollection();

    function setResponse(data, status = 200, message = "OK") {
        getContext().getResponse().setBody({ status, message, data });
    }

    var isAccepted = collection.queryDocuments(
        collection.getSelfLink(),
        `SELECT * FROM c WHERE c.id = '${tableId}'`,
        function (err, feed, options) {
            if (err) throw err;

            if (!feed || !feed.length) {
                return setResponse(null, 404, "Table not found");
            }

            feed[0].active = active

            collection.replaceDocument(feed[0]._self, feed[0], function (err, itemReplaced) {
                if (err) throw err;
                setResponse(itemReplaced);
            })
        }
    )

    if (!isAccepted) throw new Error("The query was not accepted by the server");
}