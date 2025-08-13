import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { LoadingSkeleton } from "./LoadingSkeleton";
import { SearchAndFilter } from "./SearchAndFilter";
import { useToast } from "./ToastContext";

function Challenges() {
  const [challenges, setChallenges] = useState([]);
  const [filteredChallenges, setFilteredChallenges] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const { showError } = useToast();
  const apiUrl = import.meta.env.VITE_API_URL;

  useEffect(() => {
    Promise.all([
      fetch(apiUrl + "/challenges").then(res => res.json()),
      fetch(apiUrl + "/categories").then(res => res.json())
    ])
    .then(([challengesData, categoriesData]) => {
      // Remove challenges with IsTakenDown in the past
      // SKALL REFAKTORISERAS FÖR EFFEKTIV DELETIONS OF OLD CHALLENGES AT SCALE
      const now = new Date();
      const validChallenges = [];
      const deletePromises = [];

      challengesData.forEach(challenge => {
        if (challenge.isTakenDown && new Date(challenge.isTakenDown) < now) {
          // Call your delete endpoint
          deletePromises.push(
            fetch(`${apiUrl}/challenges/${challenge.challengeId}`, { method: "DELETE" })
          );
        } else {
          validChallenges.push(challenge);
        }
      });

      // Wait for all deletes to finish, then update state
      Promise.all(deletePromises).then(() => {
        setChallenges(validChallenges);
        setFilteredChallenges(validChallenges);
        setCategories(categoriesData);
        setLoading(false);
      });
    })
    .catch(err => {
      showError("Failed to load challenges");
      setLoading(false);
    });
  }, [showError]);

  const now = new Date();
  const openChallenges = filteredChallenges.filter(ch => new Date(ch.endDate) > now);
  const closedChallenges = filteredChallenges.filter(ch => new Date(ch.endDate) <= now && new Date(ch.votePeriodEnd) > now);
  const finishedChallenges = filteredChallenges.filter(ch => new Date(ch.votePeriodEnd) <= now);

  const ChallengeCard = ({ challenge, index }) => (
    <div 
      className="card challenge-card stagger-item" 
      onClick={() => navigate(`/challenges/${challenge.challengeId}`)}
      style={{ animationDelay: `${index * 0.1}s` }}
    >
      {challenge.subCategory?.imagePath && (
        <img
          src={`${apiUrl}/${challenge.subCategory.imagePath}`}
          alt={challenge.subCategory.subCategoryName || "Category"}
          loading="lazy"
        />
      )}
      <div className="challenge-title">{challenge.challengeName}</div>
      <div className="challenge-meta">
        Ends: {new Date(challenge.endDate).toLocaleDateString()}
      </div>
      <div className="challenge-meta">
        By: {challenge.creatorUserName}
      </div>
    </div>
  );

  return (
    <div className="container fade-in">
      <SearchAndFilter
        challenges={challenges}
        onFilteredChallenges={setFilteredChallenges}
        categories={categories}
      />

      <div className="three-column-challenges">
        {/* Open Challenges */}
        <div className="challenge-column">
          <h2 className="challenge-header" style={{ color: "var(--primary-color)" }}>
            🔥 Open ({openChallenges.length})
          </h2>
          {openChallenges.length === 0 ? (
            <div className="card bounce-in" style={{ textAlign: "center", padding: "2rem" }}>
              <p>No open challenges found.</p>
              <button className="btn btn-primary pulse">Create First Challenge</button>
            </div>
          ) : (
            openChallenges.map((ch, index) => (
              <ChallengeCard key={ch.challengeId} challenge={ch} index={index} />
            ))
          )}
        </div>

        {/* Closed Challenges */}
        <div className="challenge-column">
          <h2 className="challenge-header" style={{ color: "#666" }}>
            ✅ Closed ({closedChallenges.length})
          </h2>
          {closedChallenges.length === 0 ? (
            <div className="card bounce-in" style={{ textAlign: "center", padding: "2rem" }}>
              <p>No closed challenges found.</p>
            </div>
          ) : (
            closedChallenges.map((ch, index) => (
              <ChallengeCard key={ch.challengeId} challenge={ch} index={index} />
            ))
          )}
        </div>

        {/* Finished Challenges */}
        <div className="challenge-column">
          <h2 className="challenge-header" style={{ color: "#999" }}>
            🏁 Finished ({finishedChallenges.length})
          </h2>
          {finishedChallenges.length === 0 ? (
            <div className="card bounce-in" style={{ textAlign: "center", padding: "2rem" }}>
              <p>No finished challenges found.</p>
            </div>
          ) : (
            finishedChallenges.map((ch, index) => (
              <ChallengeCard key={ch.challengeId} challenge={ch} index={index} />
            ))
          )}
        </div>


      {/* <div className="two-column-layout">

        <div className="slide-in-left">
          <h2 style={{ textAlign: "center", color: "var(--primary-color)" }}>
            🔥 Open Challenges ({openChallenges.length})
          </h2>
          {openChallenges.length === 0 ? (
            <div className="card bounce-in" style={{ textAlign: "center", padding: "2rem" }}>
              <p>No open challenges found.</p>
              <button className="btn btn-primary pulse">Create First Challenge</button>
            </div>
          ) : (
            <div className="challenges-grid">
              {openChallenges.map((ch, index) => (
                <ChallengeCard key={ch.challengeId} challenge={ch} index={index} />
              ))}
            </div>
          )}
        </div>

        <div className="slide-in-right">
          <h2 style={{ textAlign: "center", color: "#666" }}>
            ✅ Closed Challenges ({closedChallenges.length})
          </h2>
          {closedChallenges.length === 0 ? (
            <div className="card bounce-in" style={{ textAlign: "center", padding: "2rem" }}>
              <p>No closed challenges found.</p>
            </div>
          ) : (
            <div className="challenges-grid">
              {closedChallenges.map((ch, index) => (
                <ChallengeCard key={ch.challengeId} challenge={ch} index={index} />
              ))}
            </div>
          )}
        </div>

        <div className="slide-in-bottom" style={{ marginTop: "2rem" }}>
          <h2 style={{ textAlign: "center", color: "#999" }}>
            🏁 Finished Challenges ({finishedChallenges.length})
          </h2>
          {finishedChallenges.length === 0 ? (
            <div className="card bounce-in" style={{ textAlign: "center", padding: "2rem" }}>
              <p>No finished challenges found.</p>
            </div>
          ) : (
            <div className="challenges-grid">
              {finishedChallenges.map((ch, index) => (
                <ChallengeCard key={ch.challengeId} challenge={ch} index={index} />
              ))}
            </div>
          )}
        </div> */}

      </div>
    </div>
  );
}

export default Challenges;