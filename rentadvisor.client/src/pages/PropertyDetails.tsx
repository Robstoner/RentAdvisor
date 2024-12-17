import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from '../api/axiosConfig';
import '../css/PropertyDetails.css';
import '../css/PropertyCard.css';

type Review = {
    id: string;
    title: string;
    description: string;
    userId: string;
    propertyId: string;
    createdAt: string;
    updatedAt: string;
};

type Property = {
    id: string;
    name: string;
    address: string;
    description: string;
    features: string[];
    createdAt: string;
    updatedAt: string;
};

type User = {
    id: string;
    name: string;
    email: string;
};

const PropertyDetails: React.FC = () => {
    const { propertyId } = useParams<{ propertyId: string }>();
    const [property, setProperty] = useState<Property | null>(null);
    const [reviews, setReviews] = useState<Review[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [newReview, setNewReview] = useState<{ title: string; description: string }>({
        title: '',
        description: ''
    });
    const [submittingReview, setSubmittingReview] = useState<boolean>(false);
    const [currentUser, setCurrentUser] = useState<User | null>(null);
    const navigate = useNavigate();

    // Fetch current user information
    useEffect(() => {
        const fetchCurrentUser = async () => {
            try {
                const response = await axios.get<User>('/User/current');
                setCurrentUser(response.data);
            } catch (error) {
                setError('Failed to fetch current user information.');
                console.error('Error fetching current user:', error);
            }
        };

        fetchCurrentUser();
    }, []);

    // Fetch property details and reviews
    useEffect(() => {
        const fetchPropertyDetails = async () => {
            try {
                const [propertyResponse, reviewsResponse] = await Promise.all([
                    axios.get<Property>(`/api/Properties/${propertyId}`),
                    axios.get<Review[]>(`/api/Properties/${propertyId}/Reviews`)
                ]);
                setProperty(propertyResponse.data);
                setReviews(reviewsResponse.data);
            } catch (error) {
                setError('Failed to fetch property details or reviews.');
                console.error('Error fetching property details or reviews:', error);
            } finally {
                setLoading(false);
            }
        };

        if (propertyId) {
            fetchPropertyDetails();
        }
    }, [propertyId]);

    // Handle property deletion
    const handleDelete = async () => {
        try {
            await axios.delete(`/api/Properties/${propertyId}`);
            alert('Property deleted successfully!');
            navigate('/'); // Redirect to the home page after deletion
        } catch (error) {
            setError('Failed to delete property. Please try again.');
            console.error('Error deleting property:', error);
        }
    };

    // Navigate to the edit page
    const handleEdit = () => {
        navigate(`/propertyEdit/${propertyId}`);
    };

    // Handle input change for new review form
    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value } = e.target;
        setNewReview((prevReview) => ({
            ...prevReview,
            [name]: value
        }));
    };

    // Handle submission of the new review
    const handleReviewSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!newReview.title || !newReview.description) {
            setError('All fields are required to submit a review.');
            return;
        }

        setSubmittingReview(true);
        try {
            const reviewPayload = {
                ...newReview,
                propertyId,
                userId: currentUser?.id
            };

            await axios.post('/api/Reviews', reviewPayload);
            alert('Review submitted successfully!');

            // Reset form
            setNewReview({ title: '', description: '' });

            // Refresh reviews after successful submission
            const updatedReviewsResponse = await axios.get<Review[]>(`/api/Properties/${propertyId}/Reviews`);
            setReviews(updatedReviewsResponse.data);
        } catch (error) {
            setError('Failed to submit the review. Please try again.');
            console.error('Error submitting review:', error);
        } finally {
            setSubmittingReview(false);
        }
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
        <div className="main-container">
            <div className='top-content'>
                <h1 className="property-name">{property.name}</h1>
                <p className="property-address">{property.address}</p>
                <p className="property-description">{property.description}</p>
            </div>
            <div className="button-container">
                <button onClick={handleEdit} className="edit-button">Edit Property</button>
                <button onClick={handleDelete} className="delete-button">Delete Property</button>
            </div>

            <div className="property-features">
                <h3>Features:</h3>
                <ul>
                    {property.features.map((feature, index) => (
                        <li key={index}>{feature}</li>
                    ))}
                </ul>
            </div>

            <div className="property-dates">
                <p>Listed on: {new Date(property.createdAt).toLocaleDateString()}</p>
                <p>Last updated: {new Date(property.updatedAt).toLocaleDateString()}</p>
            </div>

            <div className="property-reviews">
                <h3>Reviews:</h3>
                {reviews.length > 0 ? (
                    reviews.map((review) => (
                        <div key={review.id} className="review-card">
                            <h4>{review.title}</h4>
                            <p className="review-description">{review.description}</p>
                            <p className="review-meta">
                                <strong>By:</strong> {review.userId} <br />
                                <strong>Posted on:</strong> {new Date(review.createdAt).toLocaleDateString()}
                            </p>
                        </div>
                    ))
                ) : (
                    <p>No reviews available for this property.</p>
                )}
            </div>

            <div className="new-review-form">
                <h3>Submit a Review</h3>
                <form onSubmit={handleReviewSubmit}>
                    <input
                        type="text"
                        name="title"
                        placeholder="Review Title"
                        value={newReview.title}
                        onChange={handleInputChange}
                        required
                    />
                    <textarea
                        name="description"
                        placeholder="Your Review"
                        value={newReview.description}
                        onChange={handleInputChange}
                        required
                    />
                    <div className='button-group'>
                        <button type="submit" disabled={submittingReview}>
                            {submittingReview ? 'Submitting...' : 'Submit Review'}
                        </button>
                    </div>
                </form>
            </div>

        </div>
    );
};

export default PropertyDetails;
