import React from "react";

function ClosedChallengeDetails({ challenge, user, navigate, apiUrl }) {
  return (
    <>
      {/* Display uploaded results */}
      <div style={{ marginTop: 16 }}>
        <strong>Uploaded Results:</strong>
        {challenge.uploadedResults && challenge.uploadedResults.length > 0 ? (
          <ul>
            {challenge.uploadedResults.map((result, idx) => (
              <li key={idx}>
                <span>{result.userName}</span>
                {/* Example: vote buttons */}
                <button style={{ marginLeft: 8 }}>👍</button>
                <button style={{ marginLeft: 4 }}>👎</button>
                {/* Display result info, e.g. image or link */}
                {result.imagePath && (
                  <img
                    src={`${apiUrl}/${result.imagePath}`}
                    alt="Result"
                    style={{ width: 60, height: 60, objectFit: "cover", marginLeft: 8 }}
                  />
                )}
              </li>
            ))}
          </ul>
        ) : (
          <span> No results uploaded.</span>
        )}
      </div>
    </>
  );
}

export default ClosedChallengeDetails;