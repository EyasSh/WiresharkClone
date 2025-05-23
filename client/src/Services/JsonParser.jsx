/**
 * Tries to parse the input string as JSON.
 * If successful, formats the "socket alive packet" JSON data into a human-readable string.
 * If not successful, just returns the original input string.
 * @param {string} input The input string to parse.
 * @returns {string} A human-readable string for the JSON data, or the original input string if it's not valid JSON.
 */
function JsonParser(input) {
  try {
    const json = JSON.parse(input);
    const { data: { v1, v2 } = {}, sid, ttl, type } = json;

    const uriV1 = v1?.uri ?? "N/A";
    const uriV2 = v2?.uri ?? "N/A";

    // e.g. “Socket alive packet: v1 URI = http://…, v2 URI = http://…, SID = uuid:…, TTL = 8000”
      return `Socket ${type} packet: v1 URI: ${uriV1}; v2 URI: ${uriV2}; SID: ${sid}; TTL: ${ttl}.`;
  } catch {
    // Not valid JSON—just echo back
    return input;
  }
}

export default JsonParser;
