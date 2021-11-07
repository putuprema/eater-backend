function UpsertProductCategory(payload) {
    var parsedPayload = JSON.parse(payload);
    var collection = getContext().getCollection();

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

function getResponseBody(data, status = 200, message = "OK") {
    return { status, message, data }
}

function insert(payload) {
    var collection = getContext().getCollection();

    // Check if name is already used
    var isAccepted = collection.queryDocuments(
        collection.getSelfLink(),
        `SELECT * FROM c WHERE c.name = '${payload.name}'`,
        function (err, feed, options) {
            if (err) throw err;

            if (feed && feed.length) {
                getContext().getResponse().setBody(getResponseBody(null, 409, "Category name is already used"))
            }
            else {
                getLatestSortIndex(function (sortIndex) {
                    payload.sortIndex = sortIndex;

                    var insertSuccess = collection.createDocument(collection.getSelfLink(), payload, function (err, item) {
                        if (err) throw err;
                        getContext().getResponse().setBody(getResponseBody(item))
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

    var collection = getContext().getCollection();

    // Check if name is already used
    var isAccepted = collection.queryDocuments(
        collection.getSelfLink(),
        `SELECT * FROM c WHERE c.id <> '${data.id}' AND c.name = '${data.name}'`,
        function (err, feed, options) {
            if (err) throw err;

            if (feed && feed.length) {
                getContext().getResponse().setBody(getResponseBody(null, 409, "Category name is already used"))
            }
            else {
                var updateSuccess = collection.replaceDocument(data._self, data, function (err, itemReplaced) {
                    if (err) throw err;
                    getContext().getResponse().setBody(getResponseBody(itemReplaced))
                })

                if (!updateSuccess) throw new Error("Failed to update category");
            }
        }
    )

    if (!isAccepted) throw new Error("The query was not accepted by the server");
}

function getLatestSortIndex(callback) {
    var collection = getContext().getCollection();

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