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

  useEffect(() => {
    Promise.all([
      fetch("https://localhost:7212/challenges").then(res => res.json()),
      fetch("https://localhost:7212/categories").then(res => res.json())
    ])
    .then(([challengesData, categoriesData]) => {
      setChallenges(challengesData);
      setFilteredChallenges(challengesData);
      setCategories(categoriesData);
      setLoading(false);
    })
    .catch(err => {
      showError("Failed to load challenges");
      setLoading(false);
    });
  }, [showError]);

  if (loading) return (
    <div className="container fade-in">
      <LoadingSkeleton type="card" count={6} />
    </div>
  );

  const now = new Date();
  const futureChallenges = filteredChallenges.filter(ch => new Date(ch.endDate) > now);
  const pastChallenges = filteredChallenges.filter(ch => new Date(ch.endDate) <= now);

  const ChallengeCard = ({ challenge, index }) => (
    <div 
      className="card challenge-card stagger-item" 
      onClick={() => navigate(`/challenges/${challenge.challengeId}`)}
      style={{ animationDelay: `${index * 0.1}s` }}
    >
      {challenge.subCategory?.imagePath && (
        <img
          src={`https://localhost:7212/${challenge.subCategory.imagePath}`}
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

      <div className="two-column-layout">
        <div className="slide-in-left">
          <h2 style={{ textAlign: "center", color: "var(--primary-color)" }}>
            🔥 Open Challenges ({futureChallenges.length})
          </h2>
          {futureChallenges.length === 0 ? (
            <div className="card bounce-in" style={{ textAlign: "center", padding: "2rem" }}>
              <p>No open challenges found.</p>
              <button className="btn btn-primary pulse">Create First Challenge</button>
            </div>
          ) : (
            <div className="challenges-grid">
              {futureChallenges.map((ch, index) => (
                <ChallengeCard key={ch.challengeId} challenge={ch} index={index} />
              ))}
            </div>
          )}
        </div>

        <div className="slide-in-right">
          <h2 style={{ textAlign: "center", color: "#666" }}>
            ✅ Closed Challenges ({pastChallenges.length})
          </h2>
          {pastChallenges.length === 0 ? (
            <div className="card bounce-in" style={{ textAlign: "center", padding: "2rem" }}>
              <p>No closed challenges found.</p>
            </div>
          ) : (
            <div className="challenges-grid">
              {pastChallenges.map((ch, index) => (
                <ChallengeCard key={ch.challengeId} challenge={ch} index={index} />
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default Challenges;