// import React, { useEffect, useState } from "react";

// function ArchivedChallenges() {
//   const [challenges, setChallenges] = useState([]);
//   const [loading, setLoading] = useState(true);
//   const [error, setError] = useState(null);
//   const apiUrl = import.meta.env.VITE_API_URL;

//   useEffect(() => {
//     fetch(apiUrl + "/api/archived-challenges")
//       .then((res) => {
//         if (!res.ok) throw new Error("Failed to fetch archived challenges");
//         return res.json();
//       })
//       .then((data) => {
//         setChallenges(data);
//         setLoading(false);
//       })
//       .catch((err) => {
//         setError(err.message);
//         setLoading(false);
//       });
//   }, []);

//   if (loading) return <div>Loading archived challenges...</div>;
//   if (error) return <div>Error: {error}</div>;

//   return (
//     <div>
//       <h2>Archived Challenges</h2>
//       {challenges.length === 0 ? (
//         <div>No archived challenges found.</div>
//       ) : (
//         <ul>
//           {challenges.map((challenge) => (
//             <li key={challenge.archivedChallengeId}>
//               <strong>{challenge.challengeName}</strong> <br />
//               {challenge.description} <br />
//               Subcategory: {challenge.subCategoryName} <br />
//               Ended: {new Date(challenge.endDate).toLocaleDateString()}
//             </li>
//           ))}
//         </ul>
//       )}
//     </div>
//   );
// }

// export default ArchivedChallenges;



import React, { useEffect, useState } from "react";

function ArchivedChallenges() {
  const [challenges, setChallenges] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const apiUrl = import.meta.env.VITE_API_URL;

  useEffect(() => {
    fetch(apiUrl + "/api/archived-challenges")
      .then((res) => {
        if (!res.ok) throw new Error("Failed to fetch archived challenges");
        return res.json();
      })
      .then((data) => {
        setChallenges(data);
        setLoading(false);
      })
      .catch((err) => {
        setError(err.message);
        setLoading(false);
      });
  }, []);

  if (loading) return <div>Loading archived challenges...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <div>
      <h2>Archived Challenges</h2>
      {challenges.length === 0 ? (
        <div>No archived challenges found.</div>
      ) : (
        <div style={{ overflowX: "auto" }}>
          <table style={{ width: "100%", borderCollapse: "collapse", border: "1px solid #ccc" }}>
            <thead>
              <tr>
                <th style={{ textAlign: "left", padding: "8px", border: "1px solid #ccc"}}>Challenge Name</th>
                <th style={{ textAlign: "left", padding: "8px", border: "1px solid #ccc" }}>Subcategory</th>
                <th style={{ textAlign: "left", padding: "8px", border: "1px solid #ccc" }}>End Date</th>
                <th style={{ textAlign: "left", padding: "8px", border: "1px solid #ccc" }}>Participants</th>
              </tr>
            </thead>
            <tbody>
              {challenges.map((challenge) => (
                <tr key={challenge.archivedChallengeId}>
                  <td style={{ padding: "8px", verticalAlign: "top", border: "1px solid #ccc" }}>
                    {challenge.challengeName}
                  </td>
                  <td style={{ padding: "8px", verticalAlign: "top", border: "1px solid #ccc" }}>
                    {challenge.subCategoryName}
                  </td>
                  <td style={{ padding: "8px", verticalAlign: "top", border: "1px solid #ccc" }}>
                    {new Date(challenge.endDate).toLocaleDateString('en-US', {year: 'numeric', month: 'long', day: 'numeric'})}
                  </td>
                  <td style={{ padding: "8px", verticalAlign: "top", border: "1px solid #ccc" }}>
                    {challenge.users && challenge.users.length > 0 ? (
                      <ul style={{ margin: 0, paddingLeft: "1.2em" }}>
                        {challenge.users
                          .sort((a, b) => a.placement - b.placement)
                          .map((user) => (
                            <li key={user.archivedChallengeUserId || user.userId}>
                              <span>
                                #{user.placement} {user.userName}{" "}
                                <span style={{ color: user.ratingChange >= 0 ? "green" : "red" }}>
                                  {user.ratingChange >= 0 ? "+" : ""}
                                  {user.ratingChange}
                                </span>
                              </span>
                            </li>
                          ))}
                      </ul>
                    ) : (
                      <span>No users</span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

export default ArchivedChallenges;