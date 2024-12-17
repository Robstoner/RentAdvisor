import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from '../api/axiosConfig';
import '../css/CreateProperty.css';

type Property = {
  id: string;
  name: string;
  address: string;
  description: string;
  features: string[];
  createdAt: string;
  updatedAt: string;
};

const PropertyEdit: React.FC = () => {
  const { propertyId } = useParams<{ propertyId: string }>();
  const [property, setProperty] = useState<Property | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchPropertyDetails = async () => {
      try {
        const response = await axios.get<Property>(`/api/Properties/${propertyId}`);
        setProperty(response.data);
      } catch (error) {
        setError('Failed to fetch property details.');
        console.error('Error fetching property details:', error);
      } finally {
        setLoading(false);
      }
    };

    if (propertyId) {
      fetchPropertyDetails();
    }
  }, [propertyId]);

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!property) return;

    try {
      await axios.put(`/api/Properties/${propertyId}`, property);
      alert('Property updated successfully!');
      navigate(`/property/${propertyId}`); // Redirect to the property details page
    } catch (error) {
      setError('Failed to update property. Please try again.');
      console.error('Error updating property:', error);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setProperty(prevProperty => prevProperty ? { ...prevProperty, [name]: value } : null);
  };

  if (loading) {
    return <div>Loading property details...</div>;
  }

  if (error) {
    return <div className="error-message">{error}</div>;
  }

  if (!property) {
    return <div>Property not found</div>;
  }

  return (
    <div className="property-edit-container">
      <h1>Edit Property</h1>
      <form onSubmit={handleSubmit} className="property-edit-form">
        <label htmlFor="name">Name</label>
        <input 
          type="text" 
          id="name" 
          name="name" 
          value={property.name} 
          onChange={handleChange} 
          required 
        />

        <label htmlFor="address">Address</label>
        <input 
          type="text" 
          id="address" 
          name="address" 
          value={property.address} 
          onChange={handleChange} 
          required 
        />

        <label htmlFor="description">Description</label>
        <textarea 
          id="description" 
          name="description" 
          value={property.description} 
          onChange={handleChange} 
          required 
        ></textarea>

        <label htmlFor="features">Features (comma separated)</label>
        <input 
          type="text" 
          id="features" 
          name="features" 
          value={property.features.join(', ')} 
          onChange={(e) => setProperty(prevProperty => prevProperty ? { ...prevProperty, features: e.target.value.split(',').map(f => f.trim()) } : null)} 
        />

        <button type="submit">Save Changes</button>
      </form>
    </div>
  );
};

export default PropertyEdit;
