function GenerateTables(numberOfTables) {
    var collection = getContext().getCollection();

    function setResponse(data, status = 200, message = "OK") {
        getContext().getResponse().setBody({ status, message, data });
    }

    function getLatestTableNumber(callback) {
        var isAccepted = collection.queryDocuments(
            collection.getSelfLink(),
            "SELECT TOP 1 c.number FROM c ORDER BY c.number DESC",
            function (err, feed, options) {
                if (err) throw err;

                if (!feed || !feed.length) {
                    callback(1);
                } else {
                    callback(feed[0].number + 1);
                }
            }
        )
        if (!isAccepted) throw new Error("The query was not accepted by the server");
    }

    if (numberOfTables > 20) {
        return setResponse(null, 400, "Only 20 table requests are allowed at a time");
    }

    getLatestTableNumber(number => {
        for (let i = 0; i < numberOfTables; i++) {
            var isAccepted = collection.createDocument(collection.getSelfLink(), { number: number + i, active: true, objectType: "Table", isNew: true }, function (err, item) {
                if (err) throw err;
            })
            if (!isAccepted) throw new Error("The query was not accepted by the server");
        }
        setResponse(null);
    })
}