import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";

function CreateChallenge() {
  const [challengeName, setChallengeName] = useState("");
  const [description, setDescription] = useState("");
  const [endDate, setEndDate] = useState("");
  const [numberOfParticipants, setNumberOfParticipants] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [categories, setCategories] = useState([]);
  const [subCategoryId, setSubCategoryId] = useState("");
  const [subCategories, setSubCategories] = useState([]);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const navigate = useNavigate();
  const apiUrl = import.meta.env.VITE_API_URL;

  useEffect(() => {
    fetch(apiUrl + "/categories")
      .then(res => res.ok ? res.json() : Promise.reject("Failed to fetch categories"))
      .then(data => setCategories(data))
      .catch(() => setCategories([]));
  }, []);

  const filteredSubCategories = categoryId
    ? (categories.find(cat => String(cat.categoryId) === String(categoryId))?.subCategories || [])
    : [];

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setMessage("");
    if (!challengeName || !endDate || !subCategoryId || !categoryId) {
      setError("Please fill in all required fields.");
      return;
    }
    try {
      const res = await fetch(apiUrl + "/challenges", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${localStorage.getItem("token")}`,
        },
        body: JSON.stringify({
          challengeName,
          description,
          numberOfParticipants: numberOfParticipants === "" ? 1000 : parseInt(numberOfParticipants, 10),
          endDate,
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
          <label>Number of Participants</label>
          <select
            className="form-control"
            value={numberOfParticipants || "2"}
            onChange={e => setNumberOfParticipants(e.target.value)}
            required
          >
            {[...Array(9)].map((_, i) => (
              <option key={i+2} value={i+2}>{i+2}</option>
            ))}
            <option value="1000">1000</option>
          </select>
        </div>
        <div className="form-group">
          <label>End Date*</label>
          <input
            type="date"
            className="form-control"
            value={endDate}
            onChange={e => setEndDate(e.target.value)}
            required
            min={new Date().toISOString().split("T")[0]}
          />
        </div>
        <div className="form-group">
          <label>Category*</label>
          <select
            className="form-control"
            value={categoryId}
            onChange={e => {
              setCategoryId(e.target.value);
              setSubCategoryId(""); // Reset subcategory when category changes
            }}
            required
          >
            <option value="">Select category</option>
            {categories.map(cat => (
              <option key={cat.categoryId} value={cat.categoryId}>
                {cat.categoryName}
              </option>
            ))}
          </select>
        </div>
        <div className="form-group">
          <label>Subcategory*</label>
          <select
            className="form-control"
            value={subCategoryId}
            onChange={e => setSubCategoryId(e.target.value)}
            required
            disabled={!categoryId}
          >
            <option value="">Select subcategory</option>
            {filteredSubCategories.map(sub => (
              <option key={sub.subCategoryId} value={sub.subCategoryId}>
                {sub.subCategoryName}
              </option>
            ))}
          </select>
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