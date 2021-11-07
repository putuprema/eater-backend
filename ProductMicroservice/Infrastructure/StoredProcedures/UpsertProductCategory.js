function UpsertProductCategory(payload) {
    function setResponse(data, status = 200, message = "OK") {
        getContext().getResponse().setBody({ status, message, data });
    }

    var collection = getContext().getCollection();

    function insert(payload) {
        // Check if name is already used
        var isAccepted = collection.queryDocuments(
            collection.getSelfLink(),
            `SELECT * FROM c WHERE c.name = '${payload.name}'`,
            function (err, feed, options) {
                if (err) throw err;

                if (feed && feed.length) {
                    setResponse(null, 409, "Category name is already used");
                }
                else {
                    getLatestSortIndex(function (sortIndex) {
                        payload.sortIndex = sortIndex;

                        var insertSuccess = collection.createDocument(collection.getSelfLink(), payload, function (err, item) {
                            if (err) throw err;
                            setResponse(item);
                        })

                        if (!insertSuccess) throw new Error("Failed to insert new category");
                    })
                }
            }
        )

        if (!isAccepted) throw new Error("The query was not accepted by the server");
    }

    function update(data, payload) {
        data.name = payload.name;

        // Check if name is already used
        var isAccepted = collection.queryDocuments(
            collection.getSelfLink(),
            `SELECT * FROM c WHERE c.id <> '${data.id}' AND c.name = '${data.name}'`,
            function (err, feed, options) {
                if (err) throw err;

                if (feed && feed.length) {
                    setResponse(null, 409, "Category name is already used");
                }
                else {
                    var updateSuccess = collection.replaceDocument(data._self, data, function (err, itemReplaced) {
                        if (err) throw err;
                        setResponse(itemReplaced);
                    })

                    if (!updateSuccess) throw new Error("Failed to update category");
                }
            }
        )

        if (!isAccepted) throw new Error("The query was not accepted by the server");
    }

    function getLatestSortIndex(callback) {
        var isAccepted = collection.queryDocuments(
            collection.getSelfLink(),
            "SELECT TOP 1 c.sortIndex FROM c ORDER BY c.sortIndex DESC",
            function (err, feed, options) {
                if (err) throw err;

                if (!feed || !feed.length) {
                    callback(0);
                } else {
                    callback(feed[0].sortIndex + 1);
                }
            }
        )

        if (!isAccepted) throw new Error("The query was not accepted by the server");
    }

    // Main Logic
    var parsedPayload = JSON.parse(payload);

    var isAccepted = collection.queryDocuments(
        collection.getSelfLink(),
        `SELECT * FROM c WHERE c.id = '${parsedPayload.id}'`,
        function (err, feed, options) {
            if (err) throw err;

            if (!feed || !feed.length) {
                insert(parsedPayload);
            } else {
                update(feed[0], parsedPayload);
            }
        }
    )

    if (!isAccepted) throw new Error("The query was not accepted by the server");
}