import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";

function CreateChallenge() {
  const [challengeName, setChallengeName] = useState("");
  const [description, setDescription] = useState("");
  const [endDate, setEndDate] = useState("");
  const [isPublic, setIsPublic] = useState(true);
  const [subCategoryId, setSubCategoryId] = useState("");
  const [subCategories, setSubCategories] = useState([]);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const navigate = useNavigate();

  useEffect(() => {
    // Fetch subcategories for the dropdown
    fetch("https://localhost:7212/subcategories")
      .then(res => res.ok ? res.json() : Promise.reject("Failed to fetch subcategories"))
      .then(data => setSubCategories(data))
      .catch(() => setSubCategories([]));
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setMessage("");
    if (!challengeName || !endDate || !subCategoryId) {
      setError("Please fill in all required fields.");
      return;
    }
    try {
      const res = await fetch("https://localhost:7212/challenges", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${localStorage.getItem("token")}`,
        },
        body: JSON.stringify({
          challengeName,
          description,
          endDate,
          isPublic,
          subCategoryId,
        }),
      });
      if (res.status === 201) {
        setMessage("Challenge created!");
        setTimeout(() => navigate("/"), 1200);
      } else {
        const text = await res.text();
        setError("Failed to create challenge: " + text);
      }
    } catch (err) {
      setError("Failed to create challenge: " + err.message);
    }
  };

  return (
    <div className="container fade-in" style={{ maxWidth: 500, margin: "2em auto" }}>
      <h2>Create Challenge</h2>
      <form onSubmit={handleSubmit} className="card" style={{ padding: 24 }}>
        <div className="form-group">
          <label>Challenge Name*</label>
          <input
            className="form-control"
            value={challengeName}
            onChange={e => setChallengeName(e.target.value)}
            required
          />
        </div>
        <div className="form-group">
          <label>Description</label>
          <textarea
            className="form-control"
            value={description}
            onChange={e => setDescription(e.target.value)}
            rows={3}
          />
        </div>
        <div className="form-group">
          <label>End Date*</label>
          <input
            type="date"
            className="form-control"
            value={endDate}
            onChange={e => setEndDate(e.target.value)}
            required
          />
        </div>
        <div className="form-group">
          <label>Subcategory*</label>
          <select
            className="form-control"
            value={subCategoryId}
            onChange={e => setSubCategoryId(e.target.value)}
            required
          >
            <option value="">Select subcategory</option>
            {subCategories.map(sub => (
              <option key={sub.subCategoryId} value={sub.subCategoryId}>
                {sub.subCategoryName}
              </option>
            ))}
          </select>
        </div>
        <div className="form-group">
          <label>
            <input
              type="checkbox"
              checked={isPublic}
              onChange={e => setIsPublic(e.target.checked)}
            />{" "}
            Public Challenge
          </label>
        </div>
        {error && <div style={{ color: "red", marginBottom: 8 }}>{error}</div>}
        {message && <div style={{ color: "green", marginBottom: 8 }}>{message}</div>}
        <button className="btn btn-primary" type="submit">
          Create
        </button>
      </form>
    </div>
  );
}

export default CreateChallenge;